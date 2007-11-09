//
// ShelvesetsCommand.cs
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

[Command("shelvesets", "List shelvesets in server repository.", "[<shelveset name> | <shelveset name;owner name>]")]
class ShelvesetsCommand : Command
{
	//	[Option("Format \"brief\" or \"detailed\".", "F", "format")]
	//	public string OptionFormat = "";

	[Option("Owner name", "O", "owner")]
		public string OptionOwner;

	public ShelvesetsCommand(Driver driver, string[] args): base(driver, args)
		{
		}

	public void BriefOutput(Shelveset[] shelvesets)
	{
		int maxName = 9, maxOwner = 5;
		foreach (Shelveset shelveset in shelvesets)
			{
				if (shelveset.Name.Length > maxName) maxName = shelveset.Name.Length;

				// domain is stripped on output
				int ownerNameLen = shelveset.OwnerName.Length;
				int slash = shelveset.OwnerName.IndexOf('\\');
				if (-1 != slash) ownerNameLen = shelveset.OwnerName.Length - slash;

				if (ownerNameLen > maxOwner) 
					{
						maxOwner = ownerNameLen;
					}
			}

		int maxComment = WindowWidth - maxName - maxOwner - 2;
		if (maxComment < 0) maxComment = 0;

		string line = String.Format("{0} {1} {2}", 
																"Shelveset".PadRight(maxName), 
																"Owner".PadRight(maxOwner),
																"Comment");
		Console.WriteLine(line);

		line = String.Format("{0} {1} {2}", 
												 "-".PadRight(maxName, '-'), 
												 "-".PadRight(maxOwner, '-'),
												 "-".PadRight(maxComment, '-'));

		Console.WriteLine(line);

		foreach (Shelveset shelveset in shelvesets)
			{
				string comment;
				if (shelveset.Comment.Length > maxComment) 
					comment = shelveset.Comment.Remove(maxComment);
				else
					comment = shelveset.Comment;

				// domain is stripped on output
				string ownerName = shelveset.OwnerName;
				int slash = shelveset.OwnerName.IndexOf('\\');
				if (-1 != slash)
					{
						ownerName = shelveset.OwnerName.Substring(slash+1);
					}

				line = String.Format("{0} {1} {2}", 
														 shelveset.Name.PadRight(maxName), 
														 ownerName.PadRight(maxOwner),
														 comment);
				Console.WriteLine(line);
			}
	}

	public void DetailedOutput(Shelveset[] shelvesets)
	{
		foreach (Shelveset shelveset in shelvesets)
			{
				Console.WriteLine(shelveset);
			}
	}

	public override void Run()
	{
		string name = String.Empty;
		string owner = OwnerFromString(OptionOwner);

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

		Shelveset[] shelvesets = VersionControlServer.QueryShelvesets(name, owner);

		if (shelvesets.Length == 0)
			{
				if (String.IsNullOrEmpty(name)) name = "*";
				Console.WriteLine("No shelvesets matching {0};{1}",
													name, owner);
				Environment.Exit((int)ExitCode.Failure);
			}
		
		//bool detailed = OptionFormat.Equals("detailed", StringComparison.InvariantCultureIgnoreCase);
		//if (detailed) BriefOutput(shelvesets);
		//else
		BriefOutput(shelvesets);
	}
}
