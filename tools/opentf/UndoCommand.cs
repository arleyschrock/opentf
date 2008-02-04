//
// UndoCommand.cs
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

[Command("undo", "Undo pending change(s).", "<path>...")]
class UndoCommand : Command
{
	[Option("Recursive", "R", "recursive")]
		public bool OptionRecursive = false;

	public UndoCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public override void Run()
	{
		RecursionType rtype = OptionRecursive ? RecursionType.Full : RecursionType.None;
		Workspace workspace = GetWorkspaceFromCache();
		List<string> paths = new List<string>();

		if (Arguments.Length < 1)
			{
				PendingChange[] pendingChanges = workspace.GetPendingChanges();
				foreach (PendingChange pendingChange in pendingChanges)
					{
						paths.Add(pendingChange.LocalItem);
					}
			}
		else
			{
				ConfirmFilesSpecified();
				for (int i = 0; i < Arguments.Length; i++) 
					paths.Add(Path.GetFullPath(Arguments[i]));
			}

		int rc = workspace.Undo(paths.ToArray(), rtype);
		if (rc > 0)
			Console.WriteLine("{0} file(s) undone.", rc);
		else
			Console.WriteLine("Nothing to do.");
	}
}
