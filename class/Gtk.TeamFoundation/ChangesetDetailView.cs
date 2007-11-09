using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Gtk;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Gtk.TeamFoundation
{
	public class ChangesetDetailView : ScrolledWindow, IChangesetViewChild
	{
		private Gtk.ListStore changesetDetailStore;
		private Gtk.TreeView changesetDetail;

		public ChangesetDetailView()
			{
				changesetDetail = new Gtk.TreeView();

				changesetDetail.AppendColumn ("Type", new Gtk.CellRendererText (), "text", 0);
				changesetDetail.AppendColumn ("File", new Gtk.CellRendererText (), "text", 1);

				changesetDetailStore = new Gtk.ListStore (typeof(string), typeof(string));
				changesetDetail.Model = changesetDetailStore;

#if HAVE_ATLEAST_GTK_210
				changesetDetail.EnableGridLines = TreeViewGridLines.Horizontal;
#endif

				Add(changesetDetail);
			}

		public void Clear()
		{
			changesetDetailStore.Clear();
		}

		private string ChangeTypeToString(ChangeType change)
		{
			string ctype = "edit";

			if ((change & ChangeType.Add) == ChangeType.Add) ctype = "add";
			else if ((change & ChangeType.Delete) == ChangeType.Delete) ctype = "delete";

			return ctype;
		}

		public void UpdateCid(VersionControlServer vcs, int cid)
		{
			Clear();
			Changeset changeset = vcs.GetChangeset(cid, true, false);

			foreach (Change change in changeset.Changes)
				{
					changesetDetailStore.AppendValues(ChangeTypeToString(change.ChangeType), change.Item.ServerItem);
				}
		}
	}
}