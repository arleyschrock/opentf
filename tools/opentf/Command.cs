//
// Command.cs
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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using OpenTF.Common;

abstract class Command : CommandOptions
{
	private static bool runningOnUnix = true;
	private string[] arguments;
	private Driver driver;
	private static Regex fileExcludes;
	private static Regex dirExcludes;

	public Driver Driver
	{
		get { return driver; }
	}

	public string[] Arguments
	{
		get { return arguments; }
	}

	static Command()
	{
		int p = (int) Environment.OSVersion.Platform;
		if (!((p == 4) || (p == 128))) runningOnUnix = false;

		ReadFileExcludes();
	}

	public static StringComparer PathComparer
	{
		get {
			if (!runningOnUnix) return StringComparer.CurrentCultureIgnoreCase;
			return StringComparer.CurrentCulture;
		}
	}

	public Command(Driver driver, string[] args)
	{
		this.driver = driver;
		ProcessArgs(args);
		this.arguments = RemainingArguments;
	}

	public Workspace GetWorkspaceFromServer()
	{
		string name = OptionWorkspace;

		// if option passed use it
		if (String.IsNullOrEmpty(name))
			{
				// guess based on current working directory
				WorkspaceInfo info = Workstation.Current.GetLocalWorkspaceInfo(Environment.CurrentDirectory);
				if (info != null) name = info.Name;
			}

		if (String.IsNullOrEmpty(name))
				name = Settings.Current.Get("Workspace.Default");

		if (String.IsNullOrEmpty(name))
			{
				Console.WriteLine("Unable to determine the workspace");
				Console.WriteLine("	 hint: try adding /workspace:<name>");
				Environment.Exit((int)ExitCode.Failure);
			}

		return VersionControlServer.GetWorkspace(name, Driver.Username);
	}

	public WorkspaceInfo GetWorkspaceInfoFromCache()
	{
		string path = Environment.CurrentDirectory;
		if (Arguments.Length > 0)
			{
				path = Path.GetFullPath(Arguments[0]);
			}

		WorkspaceInfo info = Workstation.Current.GetLocalWorkspaceInfo(path);
		if (info != null) return info;

		if (String.IsNullOrEmpty(OptionWorkspace))
				OptionWorkspace = Settings.Current.Get("Workspace.Default");

		if (String.IsNullOrEmpty(OptionWorkspace))
			{
				Console.WriteLine("Unable to determine the workspace.");
				Console.WriteLine("  Path: " + path);
				return null;
			}

		string ownerName = String.Format("{0}\\{1}", Driver.Domain, Driver.Username).ToUpper();
		info = Workstation.Current.GetLocalWorkspaceInfo(Driver.VersionControlServer, 
																										 OptionWorkspace, ownerName);

		if (info == null)
			{
				Console.WriteLine("Unable to determine the workspace.");
				Console.WriteLine("  Workspace Name:  " + OptionWorkspace);
				Console.WriteLine("  Workspace Owner: " + ownerName);
			}

		return info;
	}

	public Workspace GetWorkspaceFromCache()
	{
		WorkspaceInfo info = GetWorkspaceInfoFromCache();

		if (info == null)
			{
				Console.WriteLine();
				Console.WriteLine("Hints:");
				Console.WriteLine("  Try adding /workspace:<name>");
				Console.WriteLine("  Review command options prefixed with '/'. Invalid options are mistaken for paths.");
				Environment.Exit((int)ExitCode.Failure);
			}

		return VersionControlServer.GetWorkspace(info);
	}

	public VersionSpec VersionFromString(string version)
	{
		if (!String.IsNullOrEmpty(version)) 
			return VersionSpec.ParseSingleSpec(version, Driver.Username);
		else
			return VersionSpec.Latest;
	}

	static public string ChangeTypeToString(ChangeType change)
	{
		string ctype = "edit";

		if ((change & ChangeType.Add) == ChangeType.Add) ctype = "add";
		else if ((change & ChangeType.Delete) == ChangeType.Delete) ctype = "delete";
		else if ((change & ChangeType.Rename) == ChangeType.Rename) ctype = "rename";

		return ctype;
	}

	public int WindowWidth
	{
		get {
			// if output piped to a file, we don't want 0!
			// this also throws on weird terminals on msclr
			try
				{
					int w = Console.WindowWidth;
					if (w != 0) return w - 1;
				}
			catch (IOException) {}
			return 143;
		}
	}

	public string OwnerFromString(string owner)
	{
		if (String.IsNullOrEmpty(owner)) return Driver.Username;
		if (owner == "*") return null;
		return owner;
	}

	public VersionControlServer VersionControlServer
	{
		get { return Driver.VersionControlServer; }
	}
	
	public List<string> UnVerifiedFullPaths(string[] args)
	{
		List<string> paths = new List<string>();
		for (int i = 0; i < args.Length; i++) 
			{
				string fullPath = Path.GetFullPath(args[i]);
				paths.Add(fullPath);
			}
			
		return paths;
	}

	public List<string> VerifiedFullPaths(string[] args)
	{
		List<string> paths = UnVerifiedFullPaths(args);
		char[] wildcards = { '*', '?' };

		foreach (string path in paths) 
			{
				// skip wildcarded paths
				if (-1 != path.IndexOfAny(wildcards)) continue;

				if (!File.Exists(path) && !Directory.Exists(path))
					{
						Console.WriteLine("{0}: No such file or directory.", path);
						Environment.Exit((int)ExitCode.Failure);
					}
			}
			
		return paths;
	}

	public void DeleteReadOnlyFile(string fullName)
	{
		File.SetAttributes(fullName, FileAttributes.Normal);
		File.Delete(fullName);
	}

	static public void ReadFileExcludes()
	{
		string exclusionList = Settings.Current.Get("File.Excludes");
		if (String.IsNullOrEmpty(exclusionList)) return;

 		string separatorChar = Path.DirectorySeparatorChar.ToString();
		string[] wildcards = exclusionList.Split(',');

		StringBuilder dirRegexs = new StringBuilder();
		StringBuilder fileRegexs = new StringBuilder();

		foreach (string wildcard in wildcards)
			{
				if (-1 == wildcard.IndexOf(separatorChar))
					{
						if (0 != fileRegexs.Length) fileRegexs.Append("|");

						fileRegexs.Append("^");
						fileRegexs.Append(Regex.Escape(wildcard).Replace("\\*", ".*").Replace("\\?", "."));
						fileRegexs.Append("$");
					}
				else
					{
						if (0 != dirRegexs.Length) dirRegexs.Append("|");

						dirRegexs.Append("^.*");
						dirRegexs.Append(Regex.Escape(wildcard));
						dirRegexs.Append(".*$");
					}
			}

		if (0 != fileRegexs.Length)
			fileExcludes = new Regex(fileRegexs.ToString());

		if (0 != dirRegexs.Length)
			dirExcludes = new Regex(dirRegexs.ToString());
	}

	public bool IsExcludedFile(string path)
	{
		if ((fileExcludes != null) &&
				(fileExcludes.IsMatch(Path.GetFileName(path))))
			return true;

		if ((dirExcludes != null) && (dirExcludes.IsMatch(path))) 
			return true;

		return false;
	}

	public void ConfirmFilesSpecified()
	{
		if (Arguments.Length < 1)
			{
				Console.WriteLine("No files specified.");
				Environment.Exit((int)ExitCode.Failure);
			}
	}

	public string CanonicalPath(string p)
	{
		return p;
		// maybe this feature is only interesting to me?
		//if (runningOnUnix) return p;
		//return p.Replace('\\', '/');
	}

	public abstract void Run ();
}

