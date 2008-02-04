//
// WorkfoldCommand.cs
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

[Command("workfold", "Map and unmap server paths to local folders.", "<server path> [ <local path> ]")]
class WorkfoldCommand : Command
{
	[Option("Unmap", "", "unmap")]
		public bool OptionUnmap;

	public WorkfoldCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public void CurrentWorkfolders(Workspace workspace)
	{
		Console.WriteLine("=".PadRight(WindowWidth, '='));
		Console.WriteLine("Workspace: " + workspace.Name);
		Console.WriteLine("Server   : " + Driver.GetServerUrl());

		foreach (WorkingFolder folder in workspace.Folders)
			{
				Console.WriteLine("  " + folder.ServerItem + " " + folder.LocalItem);
			}
	}

	public void MapWorkfolder(Workspace workspace, string serverItem, string localItem)
	{
		List<WorkingFolder> folders = new List<WorkingFolder>(workspace.Folders);
		folders.Add(new WorkingFolder(serverItem, localItem));
		workspace.Update(workspace.Name, workspace.Comment, folders.ToArray());
	}

	public void UnmapWorkfolder(Workspace workspace, string item)
	{
		if (!	VersionControlPath.IsServerItem(item))
			item = Path.GetFullPath(item);

		List<WorkingFolder> folders = new List<WorkingFolder>(workspace.Folders);
		string msg = String.Empty;

		for (int i = 0; i < folders.Count; ++i)
			{
				WorkingFolder folder = folders[i];

				if (item == folder.ServerItem || item == folder.LocalItem)
					{
						msg = String.Format("Removed: {0} => {1}", 
																folder.ServerItem, folder.LocalItem);
						folders.RemoveAt(i);
						break;
					}
			}

		workspace.Update(workspace.Name, workspace.Comment, folders.ToArray());
		if (!String.IsNullOrEmpty(msg))
			Console.WriteLine(msg);
	}

	public override void Run()
	{
		Workspace workspace = GetWorkspaceFromServer();

		if (Arguments.Length < 1)
			CurrentWorkfolders(workspace);
		else if (Arguments.Length < 2) 
			{
				if (!OptionUnmap)
					Console.WriteLine("No-op");
				else
					UnmapWorkfolder(workspace, Arguments[0]);
			}
		else if (Arguments.Length == 2)
			MapWorkfolder(workspace, Arguments[0], Arguments[1]);
	}
}
