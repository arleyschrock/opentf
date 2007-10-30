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
using System.Security.Cryptography.X509Certificates;
using Mono.GetOptions;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;

[assembly: AssemblyTitle ("wit.exe")]
[assembly: AssemblyVersion ("0.1")]
[assembly: AssemblyDescription ("Team Foundation Work Item Tracking Tool")]
[assembly: AssemblyCopyright ("(c) Joel W. Reed")]

[assembly: Mono.UsageComplement ("")]

[assembly: Mono.About("Team Foundation Work Item Tracking Tool")]
[assembly: Mono.Author ("Joel W. Reed")]

public partial class Driver : ICertificatePolicy 
{
	private TeamFoundationServer _tfs;
	private DriverOptions Options = new DriverOptions();
	private string[] Arguments;

	private string domain;
	private string username;
	private string password;
	private string serverUrl;
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

	public string Domain
	{
		get { 
			GetUserCredentials();
			return domain; 
		}
	}

	public string Username
	{
		get { 
			GetUserCredentials();
			return username; 
		}
	}

	public string ServerUrl
	{
		get {
			if (!String.IsNullOrEmpty(serverUrl)) return serverUrl;

			if (!String.IsNullOrEmpty(Options.Server))
				{
					if (Options.Server.StartsWith("http://") || Options.Server.StartsWith("https://"))
						return Options.Server;
					else
						return String.Format("http://{0}:8080/", Options.Server);
				}

			return String.Empty;
		}
	}

	private string GetLogin()
	{
		if (!String.IsNullOrEmpty(Options.Login))
			return Options.Login;
		
		// check the keyring
		string login = TfsKeyring.GetCredentials(ServerUrl);
		if (!String.IsNullOrEmpty(login)) return login;

		// finally prompt
		return PromptForLogin(ServerUrl);
	}

	private void GetUserCredentials()
	{
		if (! String.IsNullOrEmpty(username)) return;

		string login = GetLogin();
		string userinfo = "";

		int comma = login.IndexOf(",");
		if (comma != -1)
			{
				userinfo = login.Substring(0, comma);
				password = login.Substring(comma+1);
			}
		else userinfo = login;

		int slash = userinfo.IndexOf('\\');
		if (-1 == slash) username = userinfo;
		else
			{
				domain = userinfo.Substring(0, slash);
				username = userinfo.Substring(slash+1);
			}
	}

	public NetworkCredential GetNetworkCredentials()
	{
		GetUserCredentials();
		
		if (!(String.IsNullOrEmpty(username)) && String.IsNullOrEmpty(password))
			{
				Console.Write("Password: ");
				password = Console.ReadLine();
			}

		return new NetworkCredential(username, password, domain);
	}

	public TeamFoundationServer TeamFoundationServer
	{
		get 
			{ 
				if (null != _tfs) return _tfs;
				NetworkCredential credentials = GetNetworkCredentials();
	
				_tfs = new TeamFoundationServer(ServerUrl, credentials);

				// save credentials if passed
				bool saveSetting = Settings.Current.GetAsBool("Credentials.Save");
				if (saveSetting && !String.IsNullOrEmpty(Options.Login))
					TfsKeyring.SetCredentials(ServerUrl, domain, username, password);
		
				return _tfs;
			}
	}

	public Driver(string[] args)
		{
			Arguments = args;
			Options.ProcessArgs(args);
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
				return;
			}

		Command command = (Command) Activator.CreateInstance(commandType, new object[]{ this, cmdArgs });
		command.Run();
	}

	// ignoring certificate errors
	public bool CheckValidationResult (ServicePoint sp, 
																		 X509Certificate certificate, WebRequest request, int error)
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
			}
		catch (TeamFoundationServerException e)
			{
				Console.Error.WriteLine(e.Message);
				//Console.WriteLine(driver.Domain + "\\" + driver.Username);
			}
	}
}
