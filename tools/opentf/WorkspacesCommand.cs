//
// WorkspacesCommand.cs
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Mono.GetOptions;

[Command("workspaces", "List workspaces in server repository.", "[<workspace name>]")]
class WorkspacesCommand : Command
{
	private string workspaceName;

	[Option("Computer name", "M", "computer")]
		public string OptionComputer;

	[Option("Format \"brief\" or \"detailed\".", "F", "format")]
		public string OptionFormat = "";

	[Option("Owner name", "O", "owner")]
		public string OptionOwner;

	public WorkspacesCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public void BriefOutput(Workspace[] workspaces)
	{
		int maxName = 9, maxComputer = 8, maxOwner = 5;
		foreach (Workspace workspace in workspaces)
			{
				if ((!String.IsNullOrEmpty(workspaceName)) && 
						(workspaceName != workspace.Name)) continue;

				if (workspace.Name.Length > maxName) maxName = workspace.Name.Length;
				if (workspace.Computer.Length > maxComputer) maxComputer = workspace.Computer.Length;

				// domain is stripped on output
				int ownerNameLen = workspace.OwnerName.Length;
				int slash = workspace.OwnerName.IndexOf('\\');
				if (-1 != slash) ownerNameLen = workspace.OwnerName.Length - slash;

				if (ownerNameLen > maxOwner) 
					{
						maxOwner = ownerNameLen;
					}
			}

		int maxComment = WindowWidth - maxName - maxComputer - maxOwner - 3;
		if (maxComment < 0) maxComment = 0;

		string line = String.Format("{0} {1} {2} {3}", 
																"Workspace".PadRight(maxName), 
																"Owner".PadRight(maxOwner),
																"Computer".PadRight(maxComputer), 
																"Comment");
		Console.WriteLine(line);
				
		line = String.Format("{0} {1} {2} {3}", 
												 "-".PadRight(maxName, '-'), 
												 "-".PadRight(maxOwner, '-'),
												 "-".PadRight(maxComputer, '-'), 
												 "-".PadRight(maxComment, '-'));
		Console.WriteLine(line);

		foreach (Workspace workspace in workspaces)
			{
				if ((!String.IsNullOrEmpty(workspaceName)) && 
						(workspaceName != workspace.Name)) continue;

				string comment;
				if (workspace.Comment.Length > maxComment) 
					comment = workspace.Comment.Remove(maxComment);
				else
					comment = workspace.Comment;

				// domain is stripped on output
				string ownerName = workspace.OwnerName;
				int slash = workspace.OwnerName.IndexOf('\\');
				if (-1 != slash)
					{
						ownerName = workspace.OwnerName.Substring(slash+1);
					}

				line = String.Format("{0} {1} {2} {3}", 
														 workspace.Name.PadRight(maxName), 
														 ownerName.PadRight(maxOwner),
														 workspace.Computer.PadRight(maxComputer), 
														 comment);
				Console.WriteLine(line);
			}
	}

	public void DetailedOutput(Workspace[] workspaces)
	{
		bool first = true;

		foreach (Workspace workspace in workspaces)
			{
				if ((!String.IsNullOrEmpty(workspaceName)) && 
						(workspaceName != workspace.Name)) continue;

				if (!first) Console.WriteLine();
				else first = false;

				Console.WriteLine("=".PadRight(WindowWidth, '='));
				Console.WriteLine("Workspace: " + workspace.Name);
				Console.WriteLine("Owner    : " + workspace.OwnerName);
				Console.WriteLine("Computer : " + workspace.Computer);
				Console.WriteLine("Comment  : " + workspace.Comment);
				Console.WriteLine("Server   : " + Driver.ServerUrl);

				Console.WriteLine();
				Console.WriteLine("Working folders:");
				
				foreach (WorkingFolder folder in workspace.Folders)
					{
						Console.WriteLine("  " + folder.ServerItem + " " + folder.LocalItem);
					}
			}
	}

	public string Computer
	{
		get {
			if (String.IsNullOrEmpty(OptionComputer)) return Environment.MachineName;
			if (OptionComputer == "*") return null;
			return OptionComputer;
		}
	}

	public override void Run()
	{
		if (Arguments.Length > 0) workspaceName = Arguments[0];
		Workspace[] workspaces = VersionControlServer.QueryWorkspaces(workspaceName, OwnerFromString(OptionOwner), Computer);

		if (workspaces.Length == 0)
			{
				Console.WriteLine("No workspace matching {0} on computer {1} found in Team Foundation Server.",
													OwnerFromString(OptionOwner), Computer);
				Environment.Exit((int)ExitCode.Failure);
			}
		
		bool detailed = OptionFormat.Equals("detailed", StringComparison.InvariantCultureIgnoreCase);
		if (detailed) DetailedOutput(workspaces);
		else BriefOutput(workspaces);
	}
}
