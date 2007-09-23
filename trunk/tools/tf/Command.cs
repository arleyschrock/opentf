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

abstract class Command : CommandOptions
{
	private static bool runningOnUnix = true;
	private string[] arguments;
	private Driver driver;
	private static Regex excludes;

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
		excludes = WildcardToRegex(Settings.Current.Get("File.Excludes"));
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
			{
				Console.WriteLine("Unable to determine the workspace");
				Console.WriteLine("	 hint: try adding /workspace:<name>");
				Environment.Exit(1);
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
				Console.WriteLine("  Workspace Option: " + OptionWorkspace);
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
				Environment.Exit(1);
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
						Environment.Exit(-1);
					}
			}
			
		return paths;
	}

	public void DeleteReadOnlyFile(string fullName)
	{
		File.SetAttributes(fullName, FileAttributes.Normal);
		File.Delete(fullName);
	}

	static public Regex WildcardToRegex(string exclusionList)
	{
		string[] wildcards = exclusionList.Split(',');
		StringBuilder sb = new StringBuilder();

		foreach (string wildcard in wildcards)
			{
				if (0 != sb.Length) sb.Append("|");
				string regex = "^" + Regex.Escape(wildcard).Replace("\\*", ".*").Replace("\\?", ".") + "$";
				sb.Append(regex);
			}

		return new Regex(sb.ToString());
	}

	public bool IsExcludedFile(string file)
	{
		return excludes.IsMatch(file);
	}

	public void ConfirmFilesSpecified()
	{
		if (Arguments.Length < 1)
			{
				Console.WriteLine("No files specified.");
				Environment.Exit(0);
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

