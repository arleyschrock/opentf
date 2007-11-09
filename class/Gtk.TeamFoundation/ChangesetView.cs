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
	public interface IChangesetViewChild
	{
		void UpdateCid(VersionControlServer vcs, int cid);
	}

	public class ChangesetView : Gtk.VPaned, IExploreViewChild
	{
		private Gtk.ListStore changesetListStore;
		private Gtk.TreeView changesetList;
		private ChangesetDetailView changesetDetailView;
		private ChangesetDiffView changesetDiffView;
		private Notebook viewChildren;
		private int stopAfter;
		private int currentCid = 0;
		private SortableColumns sortableColumns;
		private VersionControlServer currentVcs;

		private TreeViewColumn AppendColumn(string name, int indx)
		{
			TreeViewColumn column = changesetList.AppendColumn (name, new Gtk.CellRendererText (), "text", indx);

			column.Clickable = true;
			column.Resizable = true;
			column.SortColumnId = indx;
			column.Clicked += new EventHandler (sortableColumns.OnColumnClick);

			return column;
		}

		public void InitializeChangesetList()
		{
			changesetListStore = new Gtk.ListStore (typeof(int), typeof(string), typeof(string), typeof(string));
			changesetList = new Gtk.TreeView(changesetListStore);
			sortableColumns = new SortableColumns(changesetList, changesetListStore);

			TreeViewColumn id = AppendColumn("Id", 0);
			id.SortIndicator = true;
			id.SortOrder = SortType.Descending;

			AppendColumn("Owner", 1);
			AppendColumn("Date", 2);
			AppendColumn("Comment", 3);

			changesetList.Selection.Mode = SelectionMode.Multiple;
			changesetList.Selection.Changed += OnSelectionChanged;
			changesetList.KeyReleaseEvent += MyKeyReleaseEventHandler;

#if HAVE_ATLEAST_GTK_210
			changesetList.EnableGridLines = TreeViewGridLines.Vertical;
#endif
		}

		public ChangesetView(int stopAfter)
			{
				this.stopAfter = stopAfter;

				InitializeChangesetList();

				ScrolledWindow scrolledWindow1 = new ScrolledWindow();
				scrolledWindow1.Add(changesetList);

				Add1(scrolledWindow1);

				viewChildren = new Notebook ();
				viewChildren.TabPos = PositionType.Bottom;
				Add2(viewChildren);

				changesetDetailView = new ChangesetDetailView();
				viewChildren.AppendPage(changesetDetailView, new Label ("Details"));

				changesetDiffView = new ChangesetDiffView();
				viewChildren.AppendPage(changesetDiffView, new Label ("Unified Diff"));

				int x, y, width, height, depth;
				RootWindow.GetGeometry(out x, out y, out width, out height, out depth);
				Position = Convert.ToInt32((height - 40) * 0.4);

				viewChildren.SwitchPage += OnSwitchPage;
			}

		public void OnSwitchPage(object o, SwitchPageArgs args)
		{
			if (currentCid == 0) return;
			IChangesetViewChild child = viewChildren.GetNthPage((int)args.PageNum) as IChangesetViewChild;
			UpdateChildCid(child);
		}

		protected void UpdateChildCid(IChangesetViewChild child)
		{
			if (child == null) return;
			GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
			child.UpdateCid(currentVcs, currentCid);
			GdkWindow.Cursor = null;
		}

		void OnSelectionChanged (object o, EventArgs args)
		{
			TreeIter iter;
			TreeModel model;

			TreeSelection treeSelection = o as TreeSelection;
			int count = treeSelection.CountSelectedRows();
			if (count == 0 || count > 1)
				{
					changesetDetailView.Clear();
					changesetDiffView.Clear();
					return;
				}

			TreePath[] paths = treeSelection.GetSelectedRows(out model);
			foreach (TreePath path in paths)
				{
					model.GetIter(out iter, path);
					currentCid = Convert.ToInt32(model.GetValue (iter, 0));
				}

			IChangesetViewChild child = viewChildren.CurrentPageWidget as IChangesetViewChild;
			UpdateChildCid(child);
		}

		public void UpdatePath(VersionControlServer vcs, string path)
		{
			if (String.IsNullOrEmpty(path)) return;
			currentVcs = vcs;

			changesetListStore.Clear();
			changesetDetailView.Clear();
			changesetDiffView.Clear();

			bool detailed = false;
			IEnumerable changeSets = vcs.QueryHistory(path, VersionSpec.Latest, 0, RecursionType.Full, null, 
																																						null, null, stopAfter, detailed, false, false);

			foreach (Changeset changeSet in changeSets)
				{
					changesetListStore.AppendValues(changeSet.ChangesetId, 
																					changeSet.Owner, 
																					changeSet.CreationDate.ToString("d"),
																					changeSet.Comment);
				}

			// this would be nice be seems to cause a segfault
			//changesetList.ColumnsAutosize();
		}

		public void MyKeyReleaseEventHandler (object o, KeyReleaseEventArgs args)
		{
			if ((Gdk.Key.c == args.Event.Key) && ((args.Event.State & Gdk.ModifierType.ControlMask) != 0))
				{
					TreeIter iter; TreeModel model;
					TreePath[] paths = changesetList.Selection.GetSelectedRows(out model);

					StringBuilder sb = new StringBuilder();
					foreach (TreePath path in paths)
						{
							model.GetIter(out iter, path);
							sb.Append(String.Format("{0} {1} {2} {3}\n", 
																			Convert.ToInt32(model.GetValue (iter, 0)),
																			Convert.ToString(model.GetValue (iter, 1)),
																			Convert.ToString(model.GetValue (iter, 2)),
																			Convert.ToString(model.GetValue (iter, 3)) ));
						}

					Clipboard primary = Clipboard.Get(Gdk.Atom.Intern ("PRIMARY", false));
					primary.Text = sb.ToString();
				}
		}
	}
}