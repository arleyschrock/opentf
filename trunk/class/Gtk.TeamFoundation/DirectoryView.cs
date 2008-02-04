//
// DirectoryView.cs
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
	public class DirectoryView : TreeViewBase, IExploreViewChild
	{
		public static class ColumnIndex
		{
			public static int Name = 1;
			public static int Status = 2;
			public static int Owner = 3;
			public static int Latest = 4;
			public static int ServerPath = 5;
			public static int ItemType = 6;
		}

		private ExploreView exploreView;
		private Gtk.ListStore store;
		private SortableColumns sortableColumns;
		private VersionControlServer currentVcs;
		private DirectoryMenu menu;

		public DirectoryView(ExploreView exploreView)
			{
				this.exploreView = exploreView;
				menu = new DirectoryMenu(exploreView);

				store = new Gtk.ListStore(typeof(Gdk.Pixbuf), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(ItemType));
				Model = store;

				sortableColumns = new SortableColumns(this, store);

#if HAVE_ATLEAST_GTK_210
				EnableGridLines = TreeViewGridLines.Vertical;
#endif
				// setup main column with image/text data
				TreeViewColumn column = new TreeViewColumn ();
				CellRendererText crt = new CellRendererText();
				CellRendererPixbuf crp = new CellRendererPixbuf();
				column.Title = "Name";
				column.PackStart(crp, false);
				column.PackStart(crt, true);
				column.AddAttribute(crp, "pixbuf", 0);
				column.AddAttribute(crt, "text", 1);
				AppendColumn(column);
				AppendColumn("Status", ColumnIndex.Status);
				AppendColumn("Owner", ColumnIndex.Owner);
				AppendColumn("Latest", ColumnIndex.Latest);

				Selection.Mode = SelectionMode.Multiple;
				KeyReleaseEvent += MyKeyReleaseEventHandler;
				ButtonPressEvent += MyButtonPressEventHandler;
			}

		private TreeViewColumn AppendColumn(string name, int indx)
		{
			TreeViewColumn column = AppendColumn(name, new Gtk.CellRendererText (), "text", indx);

			column.Clickable = true;
			column.Resizable = true;
			column.SortColumnId = indx;
			column.Clicked += new EventHandler(sortableColumns.OnColumnClick);

			return column;
		}

		[GLib.ConnectBefore]
			protected void MyButtonPressEventHandler (object o, ButtonPressEventArgs args)
			{
				if (args.Event.Type == Gdk.EventType.TwoButtonPress) 
						OnShowFile();
			}

		private void OnShowFile()
		{
			TreeIter iter; TreeModel model;
			TreePath[] paths = Selection.GetSelectedRows(out model);

			if (paths.Length == 1)
				{
					model.GetIter(out iter, paths[0]);
					string serverItem = Convert.ToString(model.GetValue(iter, ColumnIndex.ServerPath));

					ItemType itemType = (ItemType) model.GetValue(iter, ColumnIndex.ItemType);
					if (itemType == ItemType.Folder)
						UpdatePath(currentVcs, serverItem);
					else
						{
							ShowFileEventArgs sfArgs = new ShowFileEventArgs(currentVcs, serverItem);
							exploreView.OnShowFile(this, sfArgs);
						}
				}
		}

		public void MyKeyReleaseEventHandler (object o, KeyReleaseEventArgs args)
		{
			if (Gdk.Key.Return == args.Event.Key)
					OnShowFile();
		}

		protected override void ShowPopupMenu(TreePath path)
		{
			//string tfpath = String.Empty;
			//Workspace workspace = null;

			if (path != null)
				{
					TreeIter iter;
					store.GetIter(out iter, path);
					//tfpath = store.GetValue(iter, ColumnIndex.Path).ToString();
				}

			//menu.Show(workspace, tfpath);
		}

		public void UpdatePath(VersionControlServer vcs, string path)
		{
			if (String.IsNullOrEmpty(path)) return;
			currentVcs = vcs;
			store.Clear();

			ExtendedItem[] items = vcs.GetExtendedItems(path, DeletedState.NonDeleted, ItemType.Any);

			foreach (ExtendedItem item in items)
				{
					if (item.TargetServerItem == path) continue;

					string shortPath = item.TargetServerItem.Substring(item.TargetServerItem.LastIndexOf('/') + 1);
					string latest = item.IsLatest ? "Yes" : "No";

					string status = item.LockStatus.ToString();
					if (status == "None") status = String.Empty;

					Gdk.Pixbuf pixbuf = Images.File;
					if (item.ItemType == ItemType.Folder)	pixbuf = Images.Folder;

					store.AppendValues(pixbuf, shortPath, status, item.LockOwner, 
														 latest, item.TargetServerItem, item.ItemType);
				}
		}
	}
}