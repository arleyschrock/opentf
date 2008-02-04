//
// CheckinCommand.cs
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
using OpenTF.Common;

[Command("checkin", "Commit pending changes to server.", "<path>...", "commit")]
class CheckinCommand : StatusCommand
{
	[Option("Specify a comment", "C", "comment")]
		public string OptionComment;

	[Option("Validate the checkin before committing", "", "validate")]
		public bool OptionValidate;

	public CheckinCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	private void UndonePendingChangeHandler(Object sender, PendingChangeEventArgs e)
	{
		Console.WriteLine("Forced Undo on " + e.PendingChange.ToString());
	}

	public override void Run()
	{
		Workspace workspace = GetWorkspaceFromCache();

		PendingChange[] pendingChanges;
		if (Arguments.Length < 1)
				pendingChanges = workspace.GetPendingChanges();
		else 
			{
				List<string> paths = new List<string>();
				for (int i = 0; i < Arguments.Length; i++) 
						paths.Add(Path.GetFullPath(Arguments[i]));

				// process command options
				RecursionType rtype = OptionRecursive ? RecursionType.Full : RecursionType.None;
				pendingChanges = workspace.GetPendingChanges(paths.ToArray(), rtype);
			}

		if (pendingChanges.Length == 0)
			{
				Console.WriteLine("Nothing to do");
				return;
			}

		bool validateSetting = Settings.Current.GetAsBool("Checkin.Validate");
		if (OptionValidate || validateSetting)
			{
				BriefOutput(pendingChanges);
				Console.Write("{0} file(s) will be committed. Continue? [y/N] ", pendingChanges.Length);
				ConsoleKeyInfo keyInfo = Console.ReadKey(true);
				Console.WriteLine();
				if (keyInfo.KeyChar != 'y') return;
			}

		bool emptyCommentSetting = Settings.Current.GetAsBool("Checkin.EmptyCommentIsFileList");
		if (String.IsNullOrEmpty(OptionComment) && emptyCommentSetting)
			{
				string cwd = Environment.CurrentDirectory;
				StringBuilder sb = new StringBuilder();
				foreach (PendingChange pendingChange in pendingChanges)
					{
						if (sb.Length > 0) sb.Append(",");

						string fname;
						if (pendingChange.LocalItem.StartsWith(cwd)) fname = pendingChange.LocalItem.Substring(cwd.Length+1);
						else fname = pendingChange.LocalItem;

						sb.Append(fname);
					}
				OptionComment = sb.ToString();
			}

		VersionControlServer.UndonePendingChange += UndonePendingChangeHandler;
		int cset = workspace.CheckIn(pendingChanges, OptionComment);
		if (cset == 0) return;

		Console.WriteLine("ChangeSet {0} checked in.", cset);
	}
}
