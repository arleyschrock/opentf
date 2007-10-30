//
// LsFilesCommand.cs
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

[Command("ls-files", "Shows known, deleted, modified, old, or unknown files under the given path.",
				 "<path>")]
class LsFilesCommand : Command
{
	[Option("Get all files, not just those out of date", "", "all")]
		public bool OptionAll = false;

	[Option("Look for deleted files.", "D", "deleted")]
		public bool OptionDeleted = false;

	[Option("Look for modified files.", "", "modified")]
		public bool OptionModified = false;

	[Option("Show out of date files (newer version on server)", "", "old")]
		public bool OptionOld = false;

	[Option("Show other/unknown files", "", "others")]
		public bool OptionOthers = false;

	[Option("Show writable files", "", "writable")]
		public bool OptionWritable;

	public LsFilesCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public void ShowModifiedFiles(string itemPath, SortedList<string, string> itemList)
	{
		DirectoryInfo dir = new DirectoryInfo(itemPath);
		MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
		foreach (FileInfo file in dir.GetFiles("*", SearchOption.AllDirectories))
			{
				if (!itemList.ContainsKey(file.FullName)) continue;

				using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
					{
						md5.ComputeHash(fileStream);
						string hash1 = Convert.ToBase64String(md5.Hash);
						
						string hash2 = itemList[file.FullName];
						if (hash1 != hash2)
							Driver.WriteLine(file.FullName);
					}				
			}
	}
	
	public void ShowOtherFiles(string itemPath, SortedList<string, string> itemList)
	{
		DirectoryInfo dir = new DirectoryInfo(itemPath);
		foreach (FileInfo file in dir.GetFiles("*", SearchOption.AllDirectories))
			{
				if (!itemList.ContainsKey(file.FullName) && !IsExcludedFile(file.FullName))
					{
						Driver.WriteLine(file.FullName);
					}
			}
	}

	public void ShowWritableFiles(string itemPath)
	{
		DirectoryInfo dir = new DirectoryInfo(itemPath);
		foreach (FileInfo file in dir.GetFiles("*", SearchOption.AllDirectories))
			{
				if (IsExcludedFile(file.FullName)) continue;

				bool isReadOnly = (FileAttributes.ReadOnly == (File.GetAttributes(file.FullName) & FileAttributes.ReadOnly));
				if (!isReadOnly) Driver.WriteLine(file.FullName);
			}
	}

	public void ShowDeletedFiles(string itemPath, SortedList<string, string> itemList)
	{
		SortedList<string, bool> dirList = new SortedList<string, bool>();
		DirectoryInfo dir = new DirectoryInfo(itemPath);

		foreach (FileInfo file in dir.GetFiles("*", SearchOption.AllDirectories))
			{
				dirList.Add(file.FullName, true);
			}
				
		foreach (DirectoryInfo di in dir.GetDirectories("*", SearchOption.AllDirectories))
			{
				dirList.Add(di.FullName, true);
			}
       
		foreach (string key in itemList.Keys)
			{
				if (!dirList.ContainsKey(key))
					{
						Driver.WriteLine(key);
					}
			}
	}

	public void ShowOldFiles(Workspace workspace, ItemSpec itemSpec)
	{
		List<ItemSpec> itemSpecs = new List<ItemSpec>();
		itemSpecs.Add(itemSpec);

		ExtendedItem[][] items = workspace.GetExtendedItems(itemSpecs.ToArray(),
																												DeletedState.NonDeleted, ItemType.Any);

		StringBuilder sb = new StringBuilder();
		foreach (ExtendedItem[] itemArray in items)
			{
				foreach (ExtendedItem item in itemArray)
					{
						if (item.IsLatest || item.ItemType == ItemType.Folder) continue;

						// new files will have a null local item
						if (String.IsNullOrEmpty(item.LocalItem))
							{
								if (OptionAll)
									sb.Append(String.Format("{0}\n", workspace.GetLocalItemForServerItem(item.SourceServerItem)));
							}
						else
							Driver.WriteLine(item.LocalItem);
					}
			}

		if (OptionAll)
			{
				Driver.WriteLine();
				Driver.WriteLine("New files on the server:");
				Driver.WriteLine();
				Driver.WriteLine(sb.ToString());
			}
	}

	public override void Run()
	{
		string path = Environment.CurrentDirectory;
		if (Arguments.Length > 0)
				path = Path.GetFullPath(Arguments[0]);

		if (File.Exists(path))
			{
				// would need to fixup dir.GetDirectories calls if we wanted to support filenames
				Console.WriteLine("Error: This command only takes paths as arguments, not file names.");
				Environment.Exit((int)ExitCode.Failure);
			}

 		char[] charsToTrim = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
		string itemPath = path.TrimEnd(charsToTrim);

		if (OptionWritable)
			{
				ShowWritableFiles(itemPath);
				Environment.Exit((int)ExitCode.Success);
			}

		Workspace workspace = GetWorkspaceFromCache();
		workspace.RefreshMappings();
		string serverPath = workspace.GetServerItemForLocalItem(itemPath);

		// process command options
		ItemSpec itemSpec = new ItemSpec(itemPath, RecursionType.Full);

		if (OptionOld)
			{
				ShowOldFiles(workspace, itemSpec);
				Environment.Exit((int)ExitCode.Success);
			}

		// pull item list based on WorkspaceVersion. otherwise might get
		// new items on server that haven't been pulled yet in the list returned
		WorkspaceVersionSpec version = new WorkspaceVersionSpec(workspace);
		ItemSet itemSet = VersionControlServer.GetItems(itemSpec, version, DeletedState.NonDeleted, ItemType.Any, true);
																									
		Item[] items = itemSet.Items;
		SortedList<string, string > itemList = new SortedList<string, string >(PathComparer);
		
		foreach (Item item in items)
			{
				if (item.ServerItem.Length == serverPath.Length) continue;
				string serverItem = item.ServerItem.Remove(0, serverPath.Length+1);

				// server item paths are separated with '/', but on windows the file list below has '\' separated paths
				if (Path.DirectorySeparatorChar != '/')
					serverItem = serverItem.Replace('/', Path.DirectorySeparatorChar);

				string fname = Path.Combine(itemPath, serverItem);
				string hash = "";

				if (item.ItemType == ItemType.File && item.HashValue != null)
					hash = Convert.ToBase64String(item.HashValue);

				itemList.Add(fname, hash);
			}

		if (OptionOthers) ShowOtherFiles(itemPath, itemList);
		else if (OptionDeleted) ShowDeletedFiles(itemPath, itemList);
		else if (OptionModified) ShowModifiedFiles(itemPath, itemList);
		else 
			{
				foreach (string key in itemList.Keys)
					{
						Driver.WriteLine(key);
					}
			}
	}
}