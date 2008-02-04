//
// BranchesCommand.cs
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

[Command("branches", "Displays the branch history for the specified path.", "<path>...")]
class BranchesCommand : Command
{
	[Option("Recursive", "R", "recursive")]
		public bool OptionRecursive = false;

	[Option("Version", "V", "Specify the version.")]
		public string OptionVersion;

	public BranchesCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public override void Run()
	{
 		RecursionType rtype = OptionRecursive ? RecursionType.Full : RecursionType.None;
		VersionSpec version = VersionFromString(OptionVersion);

		List<ItemSpec> itemSpecs = new List<ItemSpec>();
		foreach (string item in Arguments)
			{
				string fpath = (VersionControlPath.IsServerItem(item))? item : Path.GetFullPath(item);
				itemSpecs.Add(new ItemSpec(fpath, rtype));
			}

		BranchHistoryTreeItem[][] treeItemsArray = VersionControlServer.GetBranchHistory(itemSpecs.ToArray(), version);
		foreach (BranchHistoryTreeItem[] treeItems in treeItemsArray)
			{
				//Console.WriteLine("New TreeItem");
				foreach (BranchHistoryTreeItem treeItem in treeItems)
					{
						//Console.WriteLine("Relative: " + treeItem.Relative);
						//Console.WriteLine(treeItem.Relative.ServerItem);
						foreach (Object obj in treeItem.Children)
							{
								BranchRelative branch = obj as BranchRelative;
								Item fromItem = branch.BranchFromItem;
								Console.WriteLine(fromItem.ServerItem);
								Item toItem = branch.BranchToItem;
								Console.Write(">> \t" + toItem.ServerItem);
								Console.WriteLine("\tBranched from version " + fromItem.ChangesetId + " <<");
							}
					}
			}
	}
}
