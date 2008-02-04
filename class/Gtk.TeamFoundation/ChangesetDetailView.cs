//
// ChangesetDetailView.cs
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
	public class ChangesetDetailView : ScrolledWindowBase
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