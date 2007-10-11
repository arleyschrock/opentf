//
// ChangesetCommand.cs
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

[Command("changeset", "Show changeset details: committer, date, comment, and files changed.",
				 "[ <changeset id> | /latest ]")]
class ChangesetCommand : DifferenceCommand
{
	[Option("Show changeset details for the latest changeset on the server", "", "latest")]
		public bool OptionLatest;

	public ChangesetCommand(Driver driver, string[] args): base(driver, args)
		{
		}

	public override void Run()
	{
		int cid = 0;
		if (Arguments.Length > 0) cid = Convert.ToInt32(Arguments[0]);
		else if (OptionLatest) cid = VersionControlServer.GetLatestChangesetId();
	 
		if (cid == 0)
			{
				Console.WriteLine("No changeset specified.");
				Environment.Exit((int)ExitCode.Failure);
			}

		Changeset changeset = VersionControlServer.GetChangeset(cid, true, false);

		Console.WriteLine("Changeset: " + changeset.ChangesetId);
		Console.WriteLine("User: " + changeset.Committer);
		Console.WriteLine("Date: " + changeset.CreationDate);
		Console.WriteLine();

		if (!String.IsNullOrEmpty(changeset.Comment))
			{
				Console.WriteLine("Comment:");
				Console.WriteLine("  " + changeset.Comment);
				Console.WriteLine();
			}

		Console.WriteLine("Items:");
		foreach (Change change in changeset.Changes)
			{
				Console.WriteLine("  " + ChangeTypeToString(change.ChangeType) + " " + change.Item.ServerItem);
			}
	}
}
