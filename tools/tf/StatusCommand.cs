//
// StatusCommand.cs
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

public class KvpComparer: IComparer<KeyValuePair<string, string>>
{
	public int Compare(KeyValuePair<string, string> a, KeyValuePair<string, string> b)
	{
		int rc = String.Compare(a.Key, b.Key);
		if (rc != 0) return rc;
		return String.Compare(a.Value, b.Value);
	}
}

[Command("status", "Show status of pending changes in local workspace.", "<path>...", "stat")]
class StatusCommand : Command
{
	[Option("Recursive", "R", "recursive")]
		public bool OptionRecursive = false;

	public StatusCommand(Driver driver, string[] args): base(driver, args)
		{
		}

	protected void BriefOutput(PendingChange[] pendingChanges)
	{
		List<KeyValuePair<string, string>> changes = new List<KeyValuePair<string, string>>();
		foreach (PendingChange change in pendingChanges)
			{
				string ctype = change.ChangeType.ToString();
				if (change.IsAdd) ctype = "Add";
				else if (change.IsDelete) ctype = "Delete";
				else if (change.IsRename) ctype = "Rename";
				changes.Add(new KeyValuePair<string, string>(ctype, change.LocalItem));
			}

		changes.Sort(new KvpComparer());

		foreach (KeyValuePair<string, string> pair in changes)
			{
				string line = String.Format("{0}\t{1}", pair.Key, pair.Value);
				Console.WriteLine(line);
			}
	}

	public override void Run()
	{
		Workspace workspace = GetWorkspaceFromCache();
		PendingChange[] pendingChanges;

		if (Arguments.Length < 2)
			{
				pendingChanges = workspace.GetPendingChanges();
			}
		else 
			{
				List<string> paths = new List<string>();
				for (int i = 0; i < Arguments.Length; i++) 
					paths.Add(Path.GetFullPath(Arguments[i]));

				RecursionType rtype = OptionRecursive ? RecursionType.Full : RecursionType.None;
				pendingChanges = workspace.GetPendingChanges(paths.ToArray(), rtype);
			}

		BriefOutput(pendingChanges);
	}
}