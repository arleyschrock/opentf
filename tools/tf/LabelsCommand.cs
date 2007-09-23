//
//LabelsCommand.cs
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

[Command("labels", "View labels by owner or name.", "<label name>")]
class LabelsCommand : Command
{
	[Option("Format \"brief\" or \"detailed\".", "F", "format")]
		public string OptionFormat = "";

	[Option("Owner name", "O", "owner")]
		public string OptionOwner;

	public LabelsCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public void BriefOutput(VersionControlLabel[] labels)
	{
		int maxName = 9, maxOwner = 5;
		foreach (VersionControlLabel label in labels)
			{
				if (label.Name.Length > maxName) maxName = label.Name.Length;

				// domain is stripped on output
				int ownerNameLen = label.OwnerName.Length;
				int slash = label.OwnerName.IndexOf('\\');
				if (-1 != slash) ownerNameLen = label.OwnerName.Length - slash;

				if (ownerNameLen > maxOwner) 
					{
						maxOwner = ownerNameLen;
					}
			}

		int maxDate = WindowWidth - maxName - maxOwner - 3;
		if (maxDate < 0) maxDate = 0;

		string line = String.Format("{0} {1} {2}", 
																"Label".PadRight(maxName), 
																"Owner".PadRight(maxOwner),
																"Date".PadRight(maxDate));
		Console.WriteLine(line);
				
		line = String.Format("{0} {1} {2}", 
												 "-".PadRight(maxName, '-'), 
												 "-".PadRight(maxOwner, '-'),
												 "-".PadRight(maxDate, '-'));
		Console.WriteLine(line);

		foreach (VersionControlLabel label in labels)
			{
				string date = label.LastModifiedDate.ToString("d");

				// domain is stripped on output
				string ownerName = label.OwnerName;
				int slash = label.OwnerName.IndexOf('\\');
				if (-1 != slash)
					{
						ownerName = label.OwnerName.Substring(slash+1);
					}

				line = String.Format("{0} {1} {2}", 
														 label.Name.PadRight(maxName), 
														 ownerName.PadRight(maxOwner),
														 date);
				Console.WriteLine(line);
			}
	}

	public void DetailedOutput(VersionControlLabel[] labels)
	{
		bool first = true;

		foreach (VersionControlLabel label in labels)
			{
				if (!first)
					{
						Console.WriteLine("=".PadRight(WindowWidth, '='));
					}
				else first = false;

				Console.WriteLine("Label  : " + label.Name);
				Console.WriteLine("Scope  : " + label.Scope);
				Console.WriteLine("Owner  : " + label.OwnerName);
				Console.WriteLine("Date   : " + label.LastModifiedDate.ToString("F"));
				Console.WriteLine("Comment: " + label.Comment);

				Console.WriteLine();
				Console.WriteLine("Changeset Item");
				Console.WriteLine("--------- {0}", new String('-', 70));

				foreach (Item item in label.Items)
					{
						Console.WriteLine(item.ChangesetId + "      " + item.ServerItem);
					}

				Console.WriteLine();
			}
	}

	public override void Run()
	{
		string labelName = null;
		if (Arguments.Length > 0)
			labelName = Arguments[0];
			
		bool detailed = OptionFormat.Equals("detailed", StringComparison.InvariantCultureIgnoreCase);
		VersionControlLabel[] labels = VersionControlServer.QueryLabels(labelName, null, OwnerFromString(OptionOwner), detailed);

		if (detailed) DetailedOutput(labels);
		else BriefOutput(labels);
	}
}
