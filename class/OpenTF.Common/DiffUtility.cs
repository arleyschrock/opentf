using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace OpenTF.Common
{
	public sealed class DiffItemNull : IDiffItem
	{
		private string label;

		public DiffItemNull ()
			{
			}

		public string GetFile ()
		{
			return "";
		}

		public bool IsTemporary 
		{ 
			get { return true; }
		}

		public string Label 
		{ 
			get { return label; }
			set { label = value; }
		}

		public int GetEncoding()
		{
			return Encoding.UTF8.CodePage;
		}
	}

	public class DiffHelper
	{
		static public void ShowChangeset(VersionControlServer vcs, 
																		 ChangesetVersionSpec versionSpec, 
																		 bool brief, DiffOptions diffOpts)
		{
			int changesetId = versionSpec.ChangesetId;
			Changeset changeset = vcs.GetChangeset(changesetId, true, true);

			// fetch all items in one fell swoop
			List<int> ids = new List<int>();
			foreach (Change change in changeset.Changes)
				ids.Add(change.Item.ItemId);

			// find items in prior changeset
			Item[] items = vcs.GetItems(ids.ToArray(), changesetId-1, true);
			SortedList<int, Item> itemList = new SortedList<int, Item>();
			foreach (Item item in items)
				{
					// itemId of 0 means a null item, IOW file was added in this changeset
					// and missing in prior changeset
					if (item.ItemId == 0) continue;
					itemList.Add(item.ItemId, item);
				}

			foreach (Change change in changeset.Changes)
				{
					// skip folders
					if (change.Item.ItemType == ItemType.Folder) continue;
					string p = change.Item.ServerItem.Substring(2);

					if (brief) 
						{
							Console.WriteLine(p);
							continue;
						}

					IDiffItem a = new DiffItemNull();
					IDiffItem b = new DiffItemNull();

					string tnameA = null;
					string tnameB = null;

					if (((change.ChangeType & ChangeType.Add) != ChangeType.Add) &&
							(itemList.ContainsKey(change.Item.ItemId)))
						{
							Item itemA = itemList[change.Item.ItemId];

							tnameA = Path.GetTempFileName();
							itemA.DownloadFile(tnameA);

							a = new DiffItemLocalFile(tnameA, itemA.Encoding,
																				changeset.CreationDate, true);
						}

					if ((change.ChangeType & ChangeType.Delete) != ChangeType.Delete)
						{
							tnameB = Path.GetTempFileName();
							change.Item.DownloadFile(tnameB);

							b = new DiffItemLocalFile(tnameB, change.Item.Encoding,
																				changeset.CreationDate, true);
						}

					diffOpts.TargetLabel = versionSpec.DisplayString;
					Difference.DiffFiles(vcs, a, b, diffOpts, p, true);

					if (!String.IsNullOrEmpty(tnameA))
						File.Delete(tnameA);

					if (!String.IsNullOrEmpty(tnameB))
						File.Delete(tnameB);
				}
		}
	}
}