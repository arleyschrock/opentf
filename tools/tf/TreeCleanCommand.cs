//
// TreeCleanCommand.cs
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
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Mono.GetOptions;

[Command("treeclean", "Delete all files not under version control.", "<path>")]
class TreeCleanCommand : Command
{
	[Option("Preview", "", "preview")]
		public bool OptionPreview = false;

	public TreeCleanCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public override void Run()
	{
		string path = Environment.CurrentDirectory;
		if (Arguments.Length > 0)
				path = Path.GetFullPath(Arguments[0]);

		char[] charsToTrim = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
		string fxdPath = path.TrimEnd(charsToTrim);

		string itemPath = Path.Combine(path, "*"); 
		Workspace workspace = GetWorkspaceFromCache();
		workspace.RefreshMappings();
		string serverPath = workspace.GetServerItemForLocalItem(fxdPath);

		// pull item list based on WorkspaceVersion. otherwise might get
		// new items on server that haven't been pulled yet in the list returned
		WorkspaceVersionSpec version = new WorkspaceVersionSpec(workspace);

		// process command options
		ItemSpec itemSpec = new ItemSpec(itemPath, RecursionType.Full);
		ItemSet itemSet = VersionControlServer.GetItems(itemSpec, version, DeletedState.NonDeleted, ItemType.Any, true);

		Item[] items = itemSet.Items;
		SortedList<string, bool > itemList = new SortedList<string, bool >(PathComparer);
		
		foreach (Item item in items)
			{
				string serverItem = item.ServerItem.Remove(0, serverPath.Length+1);
				string fname = Path.Combine(path, serverItem);
				//Console.WriteLine(serverItem + " : " + fname);
				itemList.Add(fname, true);
			}
		
		DirectoryInfo dir = new DirectoryInfo(path);

		foreach (FileInfo file in dir.GetFiles("*", SearchOption.AllDirectories))
			{
				if (!itemList.ContainsKey(file.FullName))
					{
						if (OptionPreview) Console.WriteLine(file.FullName);
						else DeleteReadOnlyFile(file.FullName);
					}
			}
	}
}