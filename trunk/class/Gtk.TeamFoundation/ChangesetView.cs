//
// ChangesetView.cs
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
	public class ChangesetView : TreeViewBase, IExploreViewChild
	{
		private Gtk.ListStore store;
		private int stopAfter;
		private int currentCid = 0;
		private SortableColumns sortableColumns;
		private VersionControlServer currentVcs;
		private ExploreView exploreView;

		private TreeViewColumn AppendColumn(string name, int indx)
		{
			TreeViewColumn column = AppendColumn (name, new Gtk.CellRendererText (), "text", indx);

			column.Clickable = true;
			column.Resizable = true;
			column.SortColumnId = indx;
			column.Clicked += new EventHandler (sortableColumns.OnColumnClick);

			return column;
		}

		public void InitializeChangesetList()
		{
			store = new Gtk.ListStore (typeof(int), typeof(string), typeof(string), typeof(string));
			Model = store;

			sortableColumns = new SortableColumns(this, store);

			TreeViewColumn id = AppendColumn("Id", 0);
			id.SortIndicator = true;
			id.SortOrder = SortType.Descending;

			AppendColumn("Owner", 1);
			AppendColumn("Date", 2);
			AppendColumn("Comment", 3);

			Selection.Mode = SelectionMode.Multiple;
			Selection.Changed += OnSelectionChanged;
			KeyReleaseEvent += MyKeyReleaseEventHandler;
			ButtonPressEvent += MyButtonPressEventHandler;

#if HAVE_ATLEAST_GTK_210
			EnableGridLines = TreeViewGridLines.Vertical;
#endif
		}

		public ChangesetView(ExploreView exploreView, int stopAfter)
			{
				this.exploreView = exploreView;
				this.stopAfter = stopAfter;

				InitializeChangesetList();
			}

		[GLib.ConnectBefore]
			protected void MyButtonPressEventHandler (object o, ButtonPressEventArgs args)
			{
				if (args.Event.Type == Gdk.EventType.TwoButtonPress) 
					{
						ShowChangesetEventArgs scArgs = new ShowChangesetEventArgs(currentVcs, currentCid);
						exploreView.OnShowChangeset(this, scArgs);
					}
			}

		void OnSelectionChanged (object o, EventArgs args)
		{
			TreeIter iter;
			TreeModel model;

			TreeSelection treeSelection = o as TreeSelection;
			int count = treeSelection.CountSelectedRows();
			if (count == 0 || count > 1) return;

			TreePath[] paths = treeSelection.GetSelectedRows(out model);
			foreach (TreePath path in paths)
				{
					model.GetIter(out iter, path);
					currentCid = Convert.ToInt32(model.GetValue (iter, 0));
				}
		}

		public void UpdatePath(VersionControlServer vcs, string path)
		{
			if (String.IsNullOrEmpty(path)) return;
			currentVcs = vcs;
			store.Clear();

			bool detailed = false;
			IEnumerable changeSets = vcs.QueryHistory(path, VersionSpec.Latest, 0, RecursionType.Full, null, 
																								null, null, stopAfter, detailed, false, false);

			foreach (Changeset changeSet in changeSets)
				{
					store.AppendValues(changeSet.ChangesetId, 
														 changeSet.Owner, 
														 changeSet.CreationDate.ToString("d"),
														 changeSet.Comment);
				}

			// this would be nice be seems to cause a segfault
			//ColumnsAutosize();
		}

		public void MyKeyReleaseEventHandler (object o, KeyReleaseEventArgs args)
		{
			TreeIter iter; TreeModel model;
			TreePath[] paths = Selection.GetSelectedRows(out model);

			if (Gdk.Key.Return == args.Event.Key && paths.Length == 1)
				{
					ShowChangesetEventArgs scArgs = new ShowChangesetEventArgs(currentVcs, currentCid);
					exploreView.OnShowChangeset(this, scArgs);
				}
			else if ((Gdk.Key.c == args.Event.Key) && ((args.Event.State & Gdk.ModifierType.ControlMask) != 0))
				{
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