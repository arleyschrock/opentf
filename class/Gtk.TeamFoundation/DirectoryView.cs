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
	public class DirectoryView : Gtk.ScrolledWindow, IExploreViewChild
	{
		private Gtk.TreeView directoryList;
		private Gtk.ListStore directoryListStore;
		private SortableColumns sortableColumns;

		public DirectoryView()
			{
				directoryList = new Gtk.TreeView();

				directoryListStore = new Gtk.ListStore (typeof(string), typeof(string), typeof(string), typeof(string));
				directoryList.Model = directoryListStore;
				sortableColumns = new SortableColumns(directoryList, directoryListStore);

#if HAVE_ATLEAST_GTK_210
				directoryList.EnableGridLines = TreeViewGridLines.Vertical;
#endif
				AppendColumn ("Name", 0);
				AppendColumn ("Status", 1);
				AppendColumn ("Owner", 2);
				AppendColumn ("Latest", 3);

				Add(directoryList);

				int x, y, width, height, depth;
				RootWindow.GetGeometry (out x, out y, out width, out height, out depth);
			}

		private TreeViewColumn AppendColumn(string name, int indx)
		{
			TreeViewColumn column = directoryList.AppendColumn (name, new Gtk.CellRendererText (), "text", indx);

			column.Clickable = true;
			column.Resizable = true;
			column.SortColumnId = indx;
			column.Clicked += new EventHandler (sortableColumns.OnColumnClick);

			return column;
		}

		public void UpdatePath(VersionControlServer vcs, string path)
		{
			if (String.IsNullOrEmpty(path)) return;
			directoryListStore.Clear();

			ExtendedItem[] items = vcs.GetExtendedItems (path, DeletedState.NonDeleted, ItemType.Any);

			foreach (ExtendedItem item in items)
				{
					string latest = item.IsLatest ? "Yes" : "No";

					string status = item.LockStatus.ToString();
					if (status == "None") status = String.Empty;

					string shortPath = item.TargetServerItem.Substring(item.TargetServerItem.LastIndexOf('/') + 1);
					if (item.ItemType == ItemType.Folder) shortPath += "/";
				
					directoryListStore.AppendValues(shortPath, status, item.LockOwner, latest);
				}

			// this would be nice be seems to cause a segfault
			//changesetList.ColumnsAutosize();
		}
	}
}