//
// HistoryCommand.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Mono.GetOptions;

class OwnerStat : IComparable<OwnerStat>
{
	public OwnerStat(string owner, int count)
	{
		Owner = owner;
		Count = count;
	}

	public int CompareTo(OwnerStat other)
	{
		return other.Count.CompareTo(Count);
	}

	public string Owner;
	public int Count;
}

[Command("history", "Display changelog history for specified file.", "<path>...", "hist")]
class HistoryCommand : Command
{
	[Option("Format \"brief\", \"detailed\", \"byowner\"", "F", "format")]
		public string OptionFormat = "";

	[Option("Recursive", "R", "recursive")]
		public bool OptionRecursive = false;

	[Option("Limit the number of changesets shown", "", "stopafter")]
		public int OptionStopAfter = -1;

	[Option("Version of item for which to show the history", "V", "version")]
		public string OptionVersion;

	[Option("Username for filtering history results", "", "user")]
		public string OptionUser;

	public HistoryCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	private string StripDomainPrefix(string name)
	{
		int slash = name.IndexOf('\\');
		if (-1 == slash) return name;

		return name.Substring(slash + 1);
	}

	public void ByOwnerOutput(IEnumerable changeSets, int stopAfter)
	{
		Console.WriteLine("This could take some time... processing up to {0} changesets\n", stopAfter);

		int total = 0;
		int maxOwnerLen = 0;

		DateTime maxTime = new DateTime();
		Dictionary<string, int> changesByOwner = new Dictionary<string, int>();

		Changeset lastChangeSet = null;
		foreach (Changeset changeSet in changeSets)
			{
				if (total == 0) maxTime = changeSet.CreationDate;
				lastChangeSet = changeSet;

				string owner = StripDomainPrefix(changeSet.Owner);
				if (owner.Length > maxOwnerLen) maxOwnerLen = owner.Length;

				if (changesByOwner.ContainsKey(owner))
					changesByOwner[owner] += 1;
				else
					changesByOwner.Add(owner, 1);

				total++;
			}

		List<OwnerStat> ownerStats = new List<OwnerStat>();
		foreach (string owner in changesByOwner.Keys)
			{
				ownerStats.Add(new OwnerStat(owner, changesByOwner[owner]));
			}

		ownerStats.Sort();
		foreach (OwnerStat stat in ownerStats)
			{
				Console.WriteLine(stat.Owner.PadRight(maxOwnerLen) + ": " + stat.Count);
			}

		Console.WriteLine();
		Console.WriteLine("{0}: {1} to {2}", "Time Span".PadRight(maxOwnerLen), 
											lastChangeSet.CreationDate.ToString("d"), maxTime.ToString("d"));
		Console.WriteLine("{0}: {1}", "Total".PadRight(maxOwnerLen), total);
	}

	public override void Run()
	{
		bool defaultCwdSetting = Settings.Current.GetAsBool("History.DefaultToCwd");
		string path = Environment.CurrentDirectory;

		if (Arguments.Length > 0) path = Arguments[0];
		else if (!defaultCwdSetting)
			{
				Console.WriteLine("The history command takes exactly one item.");
				return;
			}

		if (!VersionControlPath.IsServerItem(path)) path = Path.GetFullPath(path);

		RecursionType rtype = OptionRecursive ? RecursionType.Full : RecursionType.None;
		bool histSetting = Settings.Current.GetAsBool("History.Recursive");
		if (histSetting) rtype = RecursionType.Full;

		bool includeChanges = Settings.Current.GetAsBool("History.Detailed");
		if (!String.IsNullOrEmpty(OptionFormat))
			includeChanges = OptionFormat.Equals("detailed", StringComparison.InvariantCultureIgnoreCase);

		int stopAfter = Settings.Current.GetAsInt("History.StopAfter");
		if (OptionStopAfter != -1) stopAfter = OptionStopAfter;

		IEnumerable changeSets = VersionControlServer.QueryHistory(path, VersionFromString(OptionVersion),
																															 0, rtype, OptionUser, 
																															 null, null, stopAfter, 
																															 includeChanges, false, false);

		if (OptionFormat.Equals("byowner", StringComparison.InvariantCultureIgnoreCase))
			{
				ByOwnerOutput(changeSets, stopAfter);
				Environment.Exit((int)ExitCode.Success);
			}

		int maxId = "Changeset".Length, maxOwner = 5, maxDate = 6;
		foreach (Changeset changeSet in changeSets)
			{
				string id = Convert.ToString(changeSet.ChangesetId);
				if (id.Length > maxId) maxId = id.Length;

				string date = changeSet.CreationDate.ToString("d");
				if (date.Length > maxDate) maxDate = date.Length;

				string name = StripDomainPrefix(changeSet.Owner);
				int ownerNameLen = name.Length;

				if (ownerNameLen > maxOwner) 
					{
						maxOwner = ownerNameLen;
					}
			}
 
		int maxComment = WindowWidth - maxId - maxOwner - maxDate - 5;

		string line = String.Format("{0} {1} {2} {3}", 
																"Changeset".PadRight(maxId), 
																"User".PadRight(maxOwner),
																"Date".PadRight(maxDate),
																"Comment".PadRight(maxComment));
		Console.WriteLine(line);
				
		line = String.Format("{0} {1} {2} {3}", 
												 "-".PadRight(maxId, '-'), 
												 "-".PadRight(maxOwner, '-'),
												 "-".PadRight(maxDate, '-'),
												 "-".PadRight(maxComment, '-'));

		Console.WriteLine(line);

		foreach (Changeset changeSet in changeSets)
			{
				string comment = "none";
				if (changeSet.Comment != null)
					{
						if (changeSet.Comment.Length > maxComment) 
							comment = changeSet.Comment.Remove(maxComment);
						else
							comment = changeSet.Comment;
					}

				// domain is stripped on output
				string owner = StripDomainPrefix(changeSet.Owner);
				line = String.Format("{0} {1} {2} {3}", 
														 Convert.ToString(changeSet.ChangesetId).PadRight(maxId), 
														 owner.PadRight(maxOwner),
														 changeSet.CreationDate.ToString("d").PadRight(maxDate), 
														 comment);

				Console.WriteLine(line);
				if (!includeChanges) continue;

				foreach (Change change in changeSet.Changes)
					{
						Console.WriteLine("  " + ChangeTypeToString(change.ChangeType) + " " + change.Item.ServerItem);
					}
			}
	}
}
