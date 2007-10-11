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

public class ChangesetDetailView : ScrolledWindow, IChangesetViewChild
{
	private Gtk.ListStore changesetDetailStore;
	private Driver driver;
	private Gtk.TreeView changesetDetail;

	public ChangesetDetailView(Driver driver)
		{
			this.driver = driver;
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

	public void UpdateCid(int cid)
	{
		Clear();
		Changeset changeset = driver.VersionControlServer.GetChangeset(cid, true, false);

		foreach (Change change in changeset.Changes)
			{
				changesetDetailStore.AppendValues(Command.ChangeTypeToString(change.ChangeType), change.Item.ServerItem);
			}
	}
}
