//
// TreeViewBase.cs
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
using System.Net;
using Gtk;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Gtk.TeamFoundation
{
	public class TreeViewBase : Gtk.TreeView
	{
		public TreeViewBase()
			{
				RowExpanded += MyRowExpandedHandler;
				ButtonPressEvent += MyButtonPressedHandler;
			}

		private void MyRowExpandedHandler (object o, RowExpandedArgs args)
		{
			PopulateRowChildren(args.Iter);
		}

		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			if (evnt.Key == Gdk.Key.Return || evnt.Key == Gdk.Key.Right)
				{
					TreeIter iter; TreeModel model;
 
					if (Selection.GetSelected (out model, out iter))
						{
							TreePath path = model.GetPath(iter);
							PopulateRowChildren(iter);
							ExpandRow(path, false);
						}
				}
			else if (evnt.Key == Gdk.Key.Left)
				{
					TreeIter iter; TreeModel model;
 
					if (Selection.GetSelected (out model, out iter))
						{
							TreePath path = model.GetPath(iter);
							CollapseRow(path);
						}
				}

			return base.OnKeyPressEvent(evnt);
		}

		protected virtual void PopulateRowChildren (TreeIter iter)
		{
		}

		protected Gtk.TreeIter SetRowValue(Gtk.TreeStore store, Gtk.TreeIter parent, 
																			 int childIndx, params object[] values)
		{
			Gtk.TreeIter child;
			if (store.IterNthChild(out child, parent, childIndx))
				{
					for (int i=0; i < values.Length; i++)
						{
							if (null == values[i]) continue;
							store.SetValue(child, i, values[i]);
						}
				}
			else
				child = store.AppendValues (parent, values);

			return child;
		}

		[GLib.ConnectBefore]
		void MyButtonPressedHandler (object o, ButtonPressEventArgs args)
		{
			if (args.Event.Button == 3)
				{
					int x = (int)args.Event.X;
					int y = (int)args.Event.Y;

					TreePath path = null;
					GetPathAtPos(x, y, out path);
					ShowPopupMenu(path);
				}
		}

		protected virtual void ShowPopupMenu(TreePath path)
		{
		}
	}
}