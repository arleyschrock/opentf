//
// ShelveCommand.cs
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

[Command("shelve", "Manage a shelveset.", "[<shelveset name> | <shelveset name;owner name>]")]
class ShelveCommand : StatusCommand
{
	[Option("Owner name", "O", "owner")]
		public string OptionOwner = RepositoryConstants.AuthenticatedUser;

	[Option("Delete the shelveset", "", "delete")]
		public bool OptionDelete = false;

	[Option("Replace an existing shelveset", "", "replace")]
		public bool OptionReplace = false;

	public ShelveCommand(Driver driver, string[] args): base(driver, args)
		{
		}

	public override void Run()
	{
		string name = String.Empty;
		string owner = OwnerFromString(OptionOwner);

		if (Arguments.Length < 1)
			{
				Console.WriteLine("Usage: tf shelve [name] [path]");
				Environment.Exit((int)ExitCode.Failure);
			}

		if (Arguments.Length > 0)
			{
				int semicolon = Arguments[0].IndexOf(";");
				if (semicolon == -1) name = Arguments[0];
				else
					{
						name = Arguments[0].Substring(0, semicolon);
						owner = Arguments[0].Substring(semicolon+1);
					}
			}

		if (OptionDelete)
			{
				VersionControlServer.DeleteShelveset(name, owner);
				Console.WriteLine("Deleted shelveset {0}", name);
				Environment.Exit((int)ExitCode.Success);
			}

		// must be creating or replacing a shelveset
		Workspace workspace = GetWorkspaceFromCache();

		PendingChange[] pendingChanges;
		if (Arguments.Length < 2)
				pendingChanges = workspace.GetPendingChanges();
		else 
			{
				List<string> paths = new List<string>();
				for (int i = 1; i < Arguments.Length; i++) 
						paths.Add(Path.GetFullPath(Arguments[i]));

				// process command options
				RecursionType rtype = OptionRecursive ? RecursionType.Full : RecursionType.None;
				pendingChanges = workspace.GetPendingChanges(paths.ToArray(), rtype);
			}

		if (pendingChanges.Length == 0)
			{
				Console.WriteLine("No changes to shelve.");
				return;
			}

		Shelveset shelve = new Shelveset(VersionControlServer, name, owner);
		ShelvingOptions options = (OptionReplace)? ShelvingOptions.Replace : ShelvingOptions.None;
		workspace.Shelve(shelve, pendingChanges, options);

		Console.WriteLine("Created shelveset {0}", name);
		BriefOutput(pendingChanges, "  ");
	}
}
