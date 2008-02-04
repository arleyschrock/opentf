//
// MergesCommand.cs
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
using OpenTF.Common;

[Command("merges", "List merges for a path.", "[<source>] <destination>")]
class MergesCommand : Command
{
	private readonly static string SourceHdr = "Changeset";
	private readonly static string TargetHdr = "Merged in Changeset";
	private readonly static string AuthorHdr = "Author";
	private readonly static string DateHdr = "Date";

	[Option("Recursive", "R", "recursive")]
		public bool OptionRecursive = false;

	public MergesCommand(Driver driver, string[] args): base(driver, args)
		{
		}

	public void BriefOutput(ChangesetMerge[] merges)
	{
		if (merges.Length == 0) return;
		int maxAuthor = 29;

		string line = String.Format("{0} {1} {2} {3}", SourceHdr, TargetHdr, 
																AuthorHdr.PadRight(maxAuthor), DateHdr);
		Console.WriteLine(line);

		line = String.Format("{0} {1} {2} {3}", "-".PadRight(SourceHdr.Length, '-'), 
												 "-".PadRight(TargetHdr.Length, '-'), 
												 "-".PadRight(maxAuthor, '-'), 
												 "-".PadRight(10, '-'));
		Console.WriteLine(line);

		foreach (ChangesetMerge merge in merges)
			{
				string srcver = merge.SourceVersion.ToString();
				string trgver = merge.TargetVersion.ToString();
				char partialCh = (merge.Partial)? '*' : ' ';

				line = String.Format("{0}{1} {2} {3} {4}", 
														 srcver.PadLeft(SourceHdr.Length-1), 
														 partialCh,
														 trgver.PadLeft(TargetHdr.Length), 
														 merge.TargetChangeset.Owner.PadRight(maxAuthor),
														 merge.TargetChangeset.CreationDate.ToString("d"));
				Console.WriteLine(line);
			}
	}

	public override void Run()
	{
		string sourcePath = String.Empty;
		string targetPath = String.Empty;
		
		switch (Arguments.Length) 
			{
			case 0:
				Console.WriteLine("Usage: tf merges [<source>] <destination>");
				Environment.Exit((int)ExitCode.Failure);
				break;
			case 1:
				targetPath = Arguments[0];
				break;
			case 2:
				sourcePath = Arguments[0];
				targetPath = Arguments[1];
				break;
			}

		if (!VersionControlPath.IsServerItem(targetPath)) 
			targetPath = Path.GetFullPath(targetPath);

		RecursionType rtype = OptionRecursive ? RecursionType.Full : RecursionType.None;
		bool setting = Settings.Current.GetAsBool("Merges.Recursive");
		if (setting) rtype = RecursionType.Full;

		ChangesetMerge[] merges = VersionControlServer.QueryMerges(sourcePath, null,
																															 targetPath, VersionSpec.Latest,
																															 null, null, rtype);

		BriefOutput(merges);
	}
}
