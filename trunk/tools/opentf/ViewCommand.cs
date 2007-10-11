//
// ViewCommand.cs
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

[Command("view", "Displays the specified version of a file.", "<path>...")]
class ViewCommand : Command
{
	[Option("Look for deleted files.", "D", "deleted")]
		public bool OptionDeleted = false;

	[Option("Recursive", "R", "recursive")]
		public bool OptionRecursive = false;

	[Option("Version", "V", "version")]
		public string OptionVersion;

	public ViewCommand(Driver driver, string[] args): base(driver, args)
		{
		}

	public override void Run()
	{
		if (Arguments.Length < 1)
			{
				Console.WriteLine("Usage: tf view <itemSpec>");
				Environment.Exit((int)ExitCode.Failure);
			}

		// process command options
		RecursionType rtype = OptionRecursive ? RecursionType.Full : RecursionType.None;
		DeletedState dstate = OptionDeleted ? DeletedState.Any : DeletedState.NonDeleted;

		List<ItemSpec> itemSpecs = new List<ItemSpec>();
		foreach (string path in Arguments)
			itemSpecs.Add(new ItemSpec(Path.GetFullPath(path), rtype));

		ItemSet[] itemSets = VersionControlServer.GetItems(itemSpecs.ToArray(), VersionFromString(OptionVersion), 
																											 dstate, ItemType.Any, true);

		foreach (ItemSet itemSet in itemSets)
			{
				foreach (Item item in itemSet.Items)
					{
						string tname = Path.GetTempFileName();
						item.DownloadFile(tname);
				
						using (StreamReader sr = new StreamReader(tname))
							{
								Console.WriteLine(sr.ReadToEnd());
							}
	
						File.Delete(tname);
					}
			}
	}
}