//
// OnlineCommand.cs
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
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Mono.GetOptions;

[Command("online", "Finds all writable files and marks them as pending changes on the server.", "<path>...")]
class OnlineCommand : Command
{
	[Option("Look for added files.", "", "added")]
		public bool OptionAdded = false;

	[Option("Look for deleted files.", "D", "deleted")]
		public bool OptionDeleted = false;

	[Option("Look for modified files.", "", "modified")]
		public bool OptionModified = false;

	[Option("Preview", "", "preview")]
		public bool OptionPreview = false;

	[Option("Recursive", "R", "recursive")]
		public bool OptionRecursive = false;

	private List<string> addedFiles = new List<string>();
	private List<string> editedFiles = new List<string>();
	private List<string> deletedFiles = new List<string>();
	private Workspace workspace;
	private MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

	public OnlineCommand(Driver driver, string[] args): base(driver, args)
		{
		}

	private void ProcessFile(SortedList<string, byte[]> itemList, string path)
	{
		bool isReadOnly = (FileAttributes.ReadOnly == (File.GetAttributes(path) & FileAttributes.ReadOnly));

		if (!itemList.ContainsKey(path))
			{
				if (OptionAdded)
					{
						Console.WriteLine("Added: " + path);
						addedFiles.Add(path);
					}
			}
		else if (!isReadOnly && OptionModified)
			{
				string itemHash = Convert.ToBase64String(itemList[path]);

				using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
					{
						md5.ComputeHash(fileStream);
						string localHash = Convert.ToBase64String(md5.Hash);
						if (itemHash != localHash)
							{
								editedFiles.Add(path);
								Console.WriteLine("Modified: " + path);
							}
					}
			}
	}

	private void ProcessFileList(SortedList<string, bool> files)
	{
		List<ItemSpec> itemSpecs = new List<ItemSpec>();
		foreach (string file in files.Keys)
				itemSpecs.Add(new ItemSpec(file, RecursionType.None));

		// pull item list based on WorkspaceVersion. otherwise might get
		// new items on server that haven't been pulled yet in the list returned
		WorkspaceVersionSpec version = new WorkspaceVersionSpec(workspace);
		SortedList<string, byte[] > itemList = new SortedList<string, byte[] >(PathComparer);

		// get item list from TFS server
		ItemSet[] itemSets = VersionControlServer.GetItems(itemSpecs.ToArray(), version, DeletedState.NonDeleted, ItemType.File, true);
		foreach (ItemSet itemSet in itemSets)
			{
				foreach (Item item in itemSet.Items)
					{
						string localItem = workspace.GetLocalItemForServerItem(item.ServerItem);
						itemList.Add(localItem, item.HashValue);
					}
			}

		// process adds and edits
		foreach (string file in files.Keys)
			{
				// skip files we're not interested in here
				if (IsExcludedFile(file)) continue;

				if (!File.Exists(file))
					{
						if (OptionDeleted && itemList.ContainsKey(file))
							{
								Console.WriteLine("Deleted: " + file);
								deletedFiles.Add(file);
							}
						continue;
					}

				ProcessFile(itemList, file);
			}
	}

	private void ProcessDirectory(string path)
	{
		char[] charsToTrim = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
		string itemPath = path.TrimEnd(charsToTrim);
		string serverPath = workspace.GetServerItemForLocalItem(itemPath);

		// pull item list based on WorkspaceVersion. otherwise might get
		// new items on server that haven't been pulled yet in the list returned
		WorkspaceVersionSpec version = new WorkspaceVersionSpec(workspace);

		// process recursion settings
		RecursionType rtype = OptionRecursive ? RecursionType.Full : RecursionType.OneLevel;
		bool recursionSetting = Settings.Current.GetAsBool("Online.Recursive");
		if (recursionSetting) rtype = RecursionType.Full;
		SearchOption searchType = (rtype == RecursionType.Full) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

		// process command options
		ItemSpec itemSpec = new ItemSpec(itemPath, rtype);
		ItemSet itemSet = VersionControlServer.GetItems(itemSpec, version, DeletedState.NonDeleted, ItemType.Any, true);

		// get item list from TFS server
		Item[] items = itemSet.Items;
		SortedList<string, byte[] > itemList = new SortedList<string, byte[] >(PathComparer);

		foreach (Item item in items)
			{
				if (item.ServerItem.Length == serverPath.Length) continue;
				string serverItem = item.ServerItem.Remove(0, serverPath.Length+1);

				// server item paths are separated with '/', but on windows the file list below has '\' separated paths
				if (Path.DirectorySeparatorChar != '/')
					serverItem = serverItem.Replace('/', Path.DirectorySeparatorChar);

				string fname = Path.Combine(itemPath, serverItem);
				//Console.WriteLine(serverItem + " : " + fname);

				itemList.Add(fname, item.HashValue);
			}
		
		DirectoryInfo dir = new DirectoryInfo(path);
		FileInfo[] localFiles = dir.GetFiles("*", searchType);

		SortedList<string, bool> dirList = new SortedList<string, bool>();
		foreach (FileInfo file in localFiles)
			{
				// skip files we're not interested in
				if (IsExcludedFile(file.Name)) continue;

				dirList.Add(file.FullName, true);
				ProcessFile(itemList, file.FullName);
			}

		foreach (DirectoryInfo di in dir.GetDirectories("*", SearchOption.AllDirectories))
			{
				dirList.Add(di.FullName, true);
			}
			 
		if (!OptionDeleted) return;

		foreach (string key in itemList.Keys)
			{
				// skip files that exist or we're not interested in
				if (dirList.ContainsKey(key)) continue;
				if (IsExcludedFile(key)) continue;

				Console.WriteLine("Deleted: " + key);
				deletedFiles.Add(key);
			}
 	}

	private void Online(string[] args)
	{
		if (args.Length == 0)
			{
				ProcessDirectory(Environment.CurrentDirectory);
				return;
			}

		SortedList<string, bool> files = new SortedList<string, bool>();
		foreach (string arg in args)
			{
				string path = Path.GetFullPath(arg);
				if (Directory.Exists(path))	ProcessDirectory(path);
				else files.Add(path, true);
			}

		if (files.Count > 0) ProcessFileList(files);
	}

	public override void Run()
	{
		// must get server<->local mappings for GetServerItemForLocalItem
		workspace = GetWorkspaceFromCache();
		workspace.RefreshMappings();

		// by default, if nothing specified we process all changes
		if ((!OptionModified) && (!OptionDeleted) && (!OptionAdded))
			{
				OptionModified = OptionAdded = OptionDeleted = true;
			}

		Online(Arguments);
		if (OptionPreview) return;

		int changes = 0;
		changes += workspace.PendAdd(addedFiles.ToArray(), false);
		changes += workspace.PendEdit(editedFiles.ToArray(), RecursionType.None);
		changes += workspace.PendDelete(deletedFiles.ToArray(), RecursionType.None);
		Console.WriteLine("{0} pending changes.", changes);
	}
}