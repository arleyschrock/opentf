//
// SortableColumns.cs
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