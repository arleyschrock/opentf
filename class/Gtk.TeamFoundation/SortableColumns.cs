using System;
using Gtk;

namespace Gtk.TeamFoundation
{
	class SortableColumns
	{
		private Gtk.TreeView view;
		private Gtk.ListStore store;
		private SortType currentSortType = SortType.Descending;
		private int currentSortColumnId = 0;

		public SortableColumns(Gtk.TreeView view, Gtk.ListStore store)
		{
			this.view = view;
			this.store = store;
		}

		public void OnColumnClick (object sender, EventArgs args)		
		{
			TreeViewColumn column = sender as TreeViewColumn;
			foreach (TreeViewColumn c in view.Columns) 
				{ 
					if (c.SortColumnId != column.SortColumnId) c.SortIndicator = false; 
				} 

			if (currentSortColumnId == column.SortColumnId)
				{
					if (currentSortType == SortType.Ascending) currentSortType = SortType.Descending;
					else currentSortType = SortType.Ascending;
				}
			else
				{
					currentSortColumnId = column.SortColumnId;
					currentSortType = SortType.Ascending;
				}

			column.SortOrder = currentSortType;
			column.SortIndicator = true; 
			store.SetSortColumnId (currentSortColumnId, currentSortType);
		}
	}
}