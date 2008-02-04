//
// RollbackCommand.cs
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

[Command("rollback", "Undo a changeset (Warning: Completely ignores any subsequent changes made after the specified changeset!)", "<changeset id>")]
class RollbackCommand : Command
{
	[Option("Preview", "", "preview")]
		public bool OptionPreview = false;

	private List<string> addedFiles = new List<string>();
	private List<string> editedFiles = new List<string>();
	private List<string> deletedFiles = new List<string>();
	private Workspace workspace;
	private int changeCount = 0;

	public RollbackCommand(Driver driver, string[] args): base(driver, args)
		{
		}

	public void ProcessEdits(Changeset changeset, int[] ids, int cid)
	{
		if (ids.Length == 0) return;

		// find items in prior changeset
		Item[] items = VersionControlServer.GetItems(ids, cid-1, true);
		SortedList<int, Item> itemList = new SortedList<int, Item>();
		foreach (Item item in items)
			{
				// itemId of 0 means a null item, IOW file was added in this changeset
				// and missing in prior changeset
				if (item.ItemId == 0) continue;
				itemList.Add(item.ItemId, item);
			}

		// fetch all items in one fell swoop
		List<string> directoryRenames = new List<string>();
		foreach (Change change in changeset.Changes)
			{
				if ((change.ChangeType & ChangeType.Add) == ChangeType.Add) continue;

				if (! itemList.ContainsKey(change.Item.ItemId))
					{
						Console.WriteLine("Cannot undo " + ChangeTypeToString(change.ChangeType) + ": " + change.Item.ServerItem);
						continue;
					}

				Item previousItem = itemList[change.Item.ItemId];
				string localItem = workspace.GetLocalItemForServerItem(previousItem.ServerItem);

				if ((change.ChangeType & ChangeType.Rename) == ChangeType.Rename)
					{
						bool fnd = false;
						foreach (string directory in directoryRenames)
							{
								if (localItem.StartsWith(directory)) 
									{
										fnd = true;
										break;
									}
							}

						// skip file renames when we've already renamed the directory
						if (fnd) continue;

						string newItem = workspace.GetLocalItemForServerItem(change.Item.ServerItem);
						Console.WriteLine("Undo rename: " + localItem + " -> " + newItem);	

						if (!OptionPreview)
							{
								workspace.PendRename(newItem, localItem);
								changeCount += 1;
							}

						if (change.Item.ItemType == ItemType.Folder)
							directoryRenames.Add(localItem);

						continue;
					}

				Console.WriteLine("Undo " + ChangeTypeToString(change.ChangeType) + ": " + change.Item.ServerItem);

				previousItem.DownloadFile(localItem);

				if ((change.ChangeType & ChangeType.Delete) == ChangeType.Delete)
					addedFiles.Add(localItem);
				else
					editedFiles.Add(localItem);
			}
	}

	public override void Run()
	{
		workspace = GetWorkspaceFromCache();
		workspace.RefreshMappings();

		if (Arguments.Length < 1)
			{
				Console.WriteLine("No changeset specified.");
				Environment.Exit((int)ExitCode.Failure);
			}

		int cid = Convert.ToInt32(Arguments[0]);
		Changeset changeset = VersionControlServer.GetChangeset(cid, true, false);

		// fetch all items in one fell swoop
		List<int> ids = new List<int>();
		foreach (Change change in changeset.Changes)
			{
				if ((change.ChangeType & ChangeType.Add) == ChangeType.Add)
					{
						if (change.Item.ItemType != ItemType.Folder)
							{
								string localItem = workspace.GetLocalItemForServerItem(change.Item.ServerItem);
								Console.WriteLine("Undo add: " + change.Item.ServerItem);
								deletedFiles.Add(localItem);
							}

						continue;
					}
				
				ids.Add(change.Item.ItemId);
			}

		ProcessEdits(changeset, ids.ToArray(), cid);

		if (OptionPreview) return;

		changeCount += workspace.PendAdd(addedFiles.ToArray(), false);
		changeCount += workspace.PendEdit(editedFiles.ToArray(), RecursionType.None);
		changeCount += workspace.PendDelete(deletedFiles.ToArray(), RecursionType.None);
		Console.WriteLine("{0} pending changes.", changeCount);
	}
}
