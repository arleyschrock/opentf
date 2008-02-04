//
// Driver.cs
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Mono.GetOptions;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Server;
using OpenTF.Common;

[assembly: AssemblyTitle ("tf.exe")]
[assembly: AssemblyVersion ("0.6.0")]
[assembly: AssemblyDescription ("Team Foundation Source Control Tool")]
[assembly: AssemblyCopyright ("(c) Joel W. Reed")]

[assembly: Mono.UsageComplement ("")]

[assembly: Mono.About("Team Foundation Source Control Tool")]
[assembly: Mono.Author ("Joel W. Reed")]

public partial class Driver : ICertificatePolicy, ICredentialsProvider
{
	private TeamFoundationServer _tfs;
	private VersionControlServer _versionControl;
	private DriverOptions Options = new DriverOptions();
	private string[] Arguments;

	private CredentialCache credentialCache = new CredentialCache();
	private List<string> outputBuffer = null;

	public void WriteLine(string x)
	{
		if (outputBuffer != null) outputBuffer.Add(x);
		else Console.WriteLine(x);
	}

	public void WriteLine()
	{
		Console.WriteLine();
	}

	public string Username
	{
		get {
			string serverUrl = GetServerUrl();
			NetworkCredential creds = credentialCache.GetCredential(new Uri(serverUrl), "NTLM");
			if (creds == null) return String.Empty;
			return creds.UserName;
		}
	}

	public string Domain
	{
		get {
			string serverUrl = GetServerUrl();
			NetworkCredential creds = credentialCache.GetCredential(new Uri(serverUrl), "NTLM");
			if (creds == null) return String.Empty;
			return creds.Domain;
		}
	}

	private string ServerNameToUrl(string server)
	{
		if (server.StartsWith("http://") || server.StartsWith("https://"))
			return server;
		else
			return String.Format("http://{0}:8080/", server);
	}

	public string GetServerUrl()
	{
		if (!String.IsNullOrEmpty(Options.Server))
			return ServerNameToUrl(Options.Server);

		WorkspaceInfo info = Workstation.Current.GetLocalWorkspaceInfo(Environment.CurrentDirectory);
		if (info == null)
			{
				string server = Settings.Current.Get("Server.Default");
				if (!String.IsNullOrEmpty(server))
					return ServerNameToUrl(server);

				Console.WriteLine("Unable to determine the team foundation server");
				Console.WriteLine("	 hint: try adding /server:<ip|name>");
				Environment.Exit((int)ExitCode.Failure);
			}

		return info.ServerUri.ToString();
	}

	private string GetLogin(string url)
	{
		if (!String.IsNullOrEmpty(Options.Login))
			return Options.Login;
		
		// check the keyring
		string login = Keyring.GetCredentials(url);
		if (!String.IsNullOrEmpty(login)) return login;

		// finally prompt if permitted
		if (Options.NoPrompt) return String.Empty;
		return PromptForLogin(url);
	}

	public TeamFoundationServer TeamFoundationServer
	{
		get 
			{
				if (null != _tfs) return _tfs;

				string url = GetServerUrl();
				ICredentials credentials = GetCredentials(new Uri(url), null);
				_tfs = new TeamFoundationServer(url, credentials);
 
				return _tfs; 
			}
	}

	// ICredentialsProvider method
	public ICredentials GetCredentials(Uri uri, ICredentials failedCredentials)
	{
		NetworkCredential creds = credentialCache.GetCredential(uri, "NTLM");
		if (creds != null) return creds;

		string url = uri.ToString();
		string login = GetLogin(url);

		if (String.IsNullOrEmpty(login)) return null;
		creds = new TFCredential(login);
		
		if (!(String.IsNullOrEmpty(creds.UserName)) && 
				String.IsNullOrEmpty(creds.Password) && !Options.NoPrompt)
			{
				Console.Write("Password: ");
				creds.Password = Console.ReadLine();
			}

		// save credentials if passed
		bool saveSetting = Settings.Current.GetAsBool("Credentials.Save");
		if (saveSetting && !String.IsNullOrEmpty(Options.Login))
			Keyring.SetCredentials(url, creds.Domain, creds.UserName, creds.Password);
		
		credentialCache.Add(uri, "NTLM", creds);
		return creds;
	}

	// ICredentialsProvider method
	public void NotifyCredentialsAuthenticated (Uri uri)
	{
	}

	public VersionControlServer VersionControlServer
	{
		get 
			{ 
				if (null != _versionControl) return _versionControl;
				_versionControl = (VersionControlServer) TeamFoundationServer.GetService(typeof(VersionControlServer));

				_versionControl.Conflict += ConflictEventHandler;
				_versionControl.NonFatalError += ExceptionEventHandler;

				return _versionControl;
			}
	}

	public Driver(string[] args)
		{
			Arguments = args;
			Options.ProcessArgs(args);
		}

	static void ConflictEventHandler(Object sender, ConflictEventArgs e)
	{
		Console.Error.WriteLine(e.Message);
	}

	static void ExceptionEventHandler(Object sender, ExceptionEventArgs e)
	{
		// sometimes e.Failure is null, not sure why yet
		if (e.Failure != null)
			Console.Error.WriteLine(e.Failure.Message);
	}

	private bool IsOption(string arg)
	{
		if (String.IsNullOrEmpty(arg)) return false;
		return (arg[0] == '-' || arg[0] == '/');
	}

	public void ProcessAllCommands() 
	{
		List<string> cmdArgs = new List<string>();

		for (int i=0; i < Arguments.Length; i++)
			{
				// a little padding between chained command output
				if (i > 0) Console.WriteLine();

				// unknown options are not sub commands
				string scmd = Arguments[i].ToLower();
				if (IsOption(scmd))
					{
						cmdArgs.Add(scmd);
						continue;
					}
				
				if (outputBuffer != null)
					{
						foreach (string line in outputBuffer)
							cmdArgs.Add(line);
						outputBuffer = null;
					}

				// pull in the rest of the args for this subcommand
				int j;
				for (j=i+1; j < Arguments.Length; j++)
					{
						string arg = Arguments[j];

						// command chaining options
						if (arg == "%") break;
						if (arg == "%%")
							{
								// buffered output across command chain
								outputBuffer = new List<string>();
								break;
							}

						// read more args from stdin
						if (arg == "-")
							{
								string line;
								if (Console.In.Peek() != -1)
									{
										while ( (line = Console.ReadLine()) != null )
											cmdArgs.Add(line);
									}

								continue;
							}
						
						cmdArgs.Add(arg);
					}

				ProcessCommand(scmd, cmdArgs.ToArray());
				cmdArgs.Clear();
				i = j;
			}
	}

	public void ProcessCommand(string cmd, string[] cmdArgs) 
	{
		Type commandType = CommandRegistry.GetCommandType(cmd);
		if (commandType == null)
			{
				Console.WriteLine("Unknown command: " + cmd);
				Environment.Exit((int)ExitCode.UnrecognizedCommand);
			}

		Command command = (Command) Activator.CreateInstance(commandType, new object[]{ this, cmdArgs });
		command.Run();
	}

	// ignoring certificate errors
	public bool CheckValidationResult (ServicePoint sp,
																		 X509Certificate certificate, WebRequest request,
																		 int error)
	{
		return true;
	}

	public static void Main(string[] args)
	{
		Driver driver = new Driver(args);
		ServicePointManager.CertificatePolicy = driver;

		if (args.Length == 0) {
			HelpCommand cmd = new HelpCommand(driver, args);
			cmd.Run();
			return;
		}

		// basic auth doesn't seem to work well on mono when POSTing
		// large data sets via a webservice. 
		AuthenticationManager.Unregister("Basic");

		try
			{
				driver.ProcessAllCommands();
				Environment.Exit((int)ExitCode.Success);
			}
		catch (TeamFoundationServerException e)
			{
				Console.Error.WriteLine(e.Message);
			}
	}
}
