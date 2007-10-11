//
// PermissionCommand.cs
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

[Command("permission", "Show server permissions on a file.", "<path>...")]
class PermissionCommand : Command
{
	[Option("Recursive", "R", "recursive")]
		public bool OptionRecursive = false;

	public PermissionCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public void WritePermString(string label, string[] perms)
	{
		Console.Write("    " + label + ": ");

		bool first = true;
		foreach (string s in perms)
			{
				if (!first) Console.Write(", ");
				else first = false;
				Console.Write(s);
			}

		Console.WriteLine();
	}

	public override void Run()
	{
		ConfirmFilesSpecified();

		List<string> items = new List<string>();
		for (int i = 0; i < Arguments.Length; i++)
			{
				string path = Arguments[i];
				if (path[0] != '$')
					{
						Console.WriteLine("Not implemented. Currently only server paths, prefixed with '$' are supported.");
						return;
					}

				items.Add(path);
			}

		// process command options
		RecursionType rtype = OptionRecursive ? RecursionType.Full : RecursionType.None;

		ItemSecurity[] itemSecurities = VersionControlServer.GetPermissions(null, items.ToArray(), 
																																				rtype);

		foreach (ItemSecurity itemSecurity in itemSecurities)
			{
				Console.WriteLine("Server item: " + itemSecurity.ServerItem);

				foreach (AccessEntry entry in itemSecurity.Entries)
					{
						Console.WriteLine();
						Console.WriteLine("  Indentity:" + entry.IdentityName);

						WritePermString("Allow", entry.Allow);
						WritePermString("Deny", entry.Deny);
						WritePermString("Allow (Inherited)", entry.AllowInherited);
						WritePermString("Deny (Inherited)", entry.DenyInherited);
					}
			}
	}
}
