//
// PropertiesCommand.cs
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
using Microsoft.TeamFoundation.VersionControl.Common;
using Mono.GetOptions;

[Command("properties", "Show detailed properties for each filename including server locks, encoding, last modified date, changeset id, and whether the file has a newer version on the server.", "<path>...")]
class PropertiesCommand : Command
{
	[Option("Look for deleted files.", "D", "deleted")]
		public bool OptionDeleted = false;

	[Option("Recursive", "R", "recursive")]
		public bool OptionRecursive = false;

	public PropertiesCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public override void Run()
	{
		Workspace workspace = GetWorkspaceFromCache();

		// process command options
		DeletedState dstate = OptionDeleted ? DeletedState.Any : DeletedState.NonDeleted;
		RecursionType rtype = OptionRecursive ? RecursionType.Full : RecursionType.OneLevel;

		List<ItemSpec> itemSpecs = new List<ItemSpec>();
		for (int i = 0; i < Arguments.Length; i++) 
			{
				string path = Arguments[i];
				if (!VersionControlPath.IsServerItem(path)) path = Path.GetFullPath(path);
				itemSpecs.Add(new ItemSpec(path, rtype));
			}
		
		ExtendedItem[][] items = workspace.GetExtendedItems(itemSpecs.ToArray(),
																												dstate, ItemType.Any);
		if (items.Length == 0)
			{
				Console.WriteLine("No items match " + Arguments[0]);
				Environment.Exit((int)ExitCode.Failure);
			}

		foreach (ExtendedItem[] itemArray in items)
			{
				foreach (ExtendedItem item in itemArray)
					{
						Console.WriteLine("Local information:");
						Console.WriteLine("	 Local path : " + item.LocalItem);
						Console.WriteLine("	 Server path: " + item.TargetServerItem);
						Console.WriteLine("	 Changeset  : " + item.VersionLocal);
						Console.WriteLine("	 Change     : " + item.ChangeType.ToString().ToLower());
						Console.WriteLine("	 Type       : " + item.ItemType.ToString().ToLower());

						Console.WriteLine("Server information:");
						Console.WriteLine("	 Server path  : " + item.SourceServerItem);
						Console.WriteLine("	 Changeset    : " + item.VersionLatest);
						Console.WriteLine("	 Deletion Id  : " + item.DeletionId);
						Console.WriteLine("	 Lock         : " + item.LockStatus.ToString().ToLower());
						Console.WriteLine("	 Lock Owner   : " + item.LockOwner);
						Console.WriteLine("	 Last Modified: Not Implemented");
						Console.WriteLine("	 Type         : " + item.ItemType.ToString().ToLower());

						Encoding encoding = Encoding.GetEncoding(item.Encoding);
						Console.WriteLine("	 File type    : " + encoding.HeaderName);

						long len = 0;
						if (!String.IsNullOrEmpty(item.LocalItem))
							{
								FileInfo fi = new FileInfo(item.LocalItem);
								len = fi.Length;
							}

						Console.WriteLine("	 Size         : " + Convert.ToString(len));
					}
			}
	}
}