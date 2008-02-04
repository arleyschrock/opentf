//
// WorkspaceCommand.cs
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
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Mono.GetOptions;

[Command("workspace", "Create and delete workspaces.", "[/new <workspace.spec> | /delete <workspace.name> ]")]
class WorkspaceCommand : Command
{
	[Option("Create a new workspace", "", "new")]
		public bool OptionNew = false;

	[Option("Delete a workspace", "", "delete")]
		public bool OptionDelete = false;

	[Option("Specify a comment", "C", "comment")]
		public string OptionComment;

	[Option("Owner name", "O", "owner")]
		public string OptionOwner;

	public WorkspaceCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public void ProcessWorkspaceNew(string args) 
	{
		int semicolon = args.IndexOf(";");
		if (semicolon == -1)
			{
				Console.WriteLine("Failed to parse argument: " + args);
				return;
			}

		string name = args.Substring(0, semicolon);
		string ownerName = args.Substring(semicolon+1);

		Console.WriteLine("Creating workspace: " + name + " for " + ownerName);
		WorkingFolder[] folders = new WorkingFolder[1];
		folders[0] = new WorkingFolder(VersionControlPath.RootFolder, Environment.CurrentDirectory);
		Console.WriteLine("Mapping \"$/\" to " + Environment.CurrentDirectory);

		VersionControlServer.CreateWorkspace(name, ownerName, OptionComment,
																				 folders, Environment.MachineName);

		return;
	}

	public void ProcessWorkspaceDelete(string arg) 
	{
		string name = arg;
		string ownerName = OwnerFromString(OptionOwner);

		int semicolon = arg.IndexOf(";");
		if (semicolon != -1)
			{
				name = arg.Substring(0, semicolon);
				ownerName = arg.Substring(semicolon+1);
			}

		if (String.IsNullOrEmpty(ownerName))
			{
				Console.WriteLine("Error: cannot determine the workspace owner.");
				Console.WriteLine("Please use /owner or specify the workspace with the \"<name>;<owner>\" syntax.");
				Environment.Exit((int)ExitCode.Failure);
			}

		Console.WriteLine("Deleting workspace: " + arg);
		VersionControlServer.DeleteWorkspace(name, ownerName);
	}

	public void Usage()
	{
		Console.WriteLine("Usage: tf workspace [/new <workspace.spec> | /delete <workspace.name> ]");
		Environment.Exit((int)ExitCode.Failure);
	}

	public override void Run()
	{
		if (Arguments.Length < 1) Usage();

		if (OptionDelete) ProcessWorkspaceDelete(Arguments[0]);
		else if (OptionNew) ProcessWorkspaceNew(Arguments[0]);
		else
			{
				Console.WriteLine("Unknown option: " + Arguments[0]);
				Usage();
			}
	}
}
