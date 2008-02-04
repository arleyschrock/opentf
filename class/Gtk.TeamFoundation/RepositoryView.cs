//
// RepositoryView.cs
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
using System.Text;
using Gtk;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Gtk.TeamFoundation
{
	public class RepositoryView : TreeViewBase
	{
		public static class ColumnIndex
		{
			public static int Url = 2;
			public static int Path = 3;
			public static int Workspace = 4;
			public static int Mapped = 5;
		}

		private ExploreView exploreView;
		private TreeStore store = new TreeStore(typeof(Gdk.Pixbuf), typeof(string), typeof(string), typeof(string), typeof(Workspace), typeof(bool));
		private ICredentialsProvider credentialsProvider;
		private RepositoryMenu menu;

		public RepositoryView(ExploreView exploreView, ICredentialsProvider credentialsProvider)
			{
				this.exploreView = exploreView;
				menu = new RepositoryMenu(exploreView);
				this.credentialsProvider = credentialsProvider;

				// setup main column with image/text data
				TreeViewColumn column = new TreeViewColumn ();
				CellRendererText crt = new CellRendererText();
				CellRendererPixbuf crp = new CellRendererPixbuf();
				column.Title = "Repository";
				column.PackStart(crp, false);
				column.PackStart(crt, true);
				column.AddAttribute(crp, "pixbuf", 0);
				column.AddAttribute(crt, "text", 1);
				column.SetCellDataFunc(crt, new Gtk.TreeCellDataFunc (RenderRepositoryName));
				AppendColumn(column);

				WorkspaceInfo[] infos = Workstation.Current.GetAllLocalWorkspaceInfo();
				foreach (WorkspaceInfo info in infos)
					{
						ICredentials credentials = credentialsProvider.GetCredentials(info.ServerUri, null);
						TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer(info.ServerUri.ToString(), credentials);
						VersionControlServer vcs = tfs.GetService(typeof(VersionControlServer)) as VersionControlServer;

						Workspace workspace = vcs.GetWorkspace(info.Name, info.OwnerName);
						workspace.RefreshMappings();

						string label = String.Format("{0}@{1}", info.Name, info.ServerUri.Host.ToString());
						Gtk.TreeIter serverIter = store.AppendValues(Images.Repository, label, info.ServerUri.ToString(), VersionControlPath.RootFolder, workspace, true);
						store.AppendValues(serverIter, null, "", "", "", null, true);
					}

				Model = store;
				HeadersVisible = true;
				KeyReleaseEvent += MyKeyReleaseEventHandler;
			
				ShowAll();
			}

		protected override void PopulateRowChildren (TreeIter iter)
		{
			string path = store.GetValue(iter, ColumnIndex.Path).ToString();

			string url = String.Empty;
			Workspace workspace = null;

			TreeIter iterParent; TreeIter current = iter;
			while (store.IterParent(out iterParent, current))
				current = iterParent;

			url = store.GetValue(current, ColumnIndex.Url).ToString();
			workspace = store.GetValue(current, ColumnIndex.Workspace) as Workspace;

			ICredentials credentials = credentialsProvider.GetCredentials(new Uri(url), null);
			TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer(url, credentials);
			VersionControlServer versionControlServer = tfs.GetService(typeof(VersionControlServer)) as VersionControlServer;

			int indx = 0;
			ItemSpec itemSpec = new ItemSpec(path, RecursionType.OneLevel);
			ItemSet itemSet = versionControlServer.GetItems(itemSpec, VersionSpec.Latest, 
																											DeletedState.NonDeleted, ItemType.Folder, false);

			foreach (Microsoft.TeamFoundation.VersionControl.Client.Item item in itemSet.Items)
				{
					if (item.ServerItem == path) continue;

					string shortPath = item.ServerItem.Substring(item.ServerItem.LastIndexOf('/') + 1);
					bool mapped = workspace.IsServerPathMapped(item.ServerItem);

					Gtk.TreeIter child = SetRowValue(store, iter, indx, Images.Folder, shortPath, 
																					 url, item.ServerItem, workspace, mapped);
					store.AppendValues(child, null, "", "", "", null, true);

					indx++;
				}

			// we didn't add anything!
			if (indx == 0)
				{
					TreeIter citer;	store.IterChildren(out citer, iter);
					store.Remove(ref citer);
				}
		}

		protected override void ShowPopupMenu(TreePath path)
		{
			string tfpath = String.Empty;
			Workspace workspace = null;

			if (path != null)
				{
					TreeIter iter;
					store.GetIter(out iter, path);

					tfpath = store.GetValue(iter, ColumnIndex.Path).ToString();
					workspace = store.GetValue(iter, ColumnIndex.Workspace) as Workspace;
				}

			menu.Show(workspace, tfpath);
		}

		private void RenderRepositoryName(Gtk.TreeViewColumn column, 
																			Gtk.CellRenderer cell, 
																			Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			string path = model.GetValue(iter, ColumnIndex.Path).ToString();
			Workspace workspace = model.GetValue(iter, ColumnIndex.Workspace) as Workspace;

			Gtk.CellRendererText crt = cell as Gtk.CellRendererText;
			crt.Foreground = "black";

			if (path == VersionControlPath.RootFolder) return;
			if (workspace == null) return;

			bool mapped = (bool) store.GetValue(iter, ColumnIndex.Mapped);
			if (!mapped) (cell as Gtk.CellRendererText).Foreground = "dark grey";
		}

		public void MyKeyReleaseEventHandler(object o, KeyReleaseEventArgs args)
		{
			TreeIter iter; TreeModel model;
			TreePath[] paths = Selection.GetSelectedRows(out model);

			if ((Gdk.Key.c == args.Event.Key) && ((args.Event.State & Gdk.ModifierType.ControlMask) != 0))
				{
					StringBuilder sb = new StringBuilder();
					foreach (TreePath path in paths)
						{
							model.GetIter(out iter, path);
							sb.Append(String.Format("{0}\n", 
																			Convert.ToString(model.GetValue(iter, 3))));
						}

					Clipboard primary = Clipboard.Get(Gdk.Atom.Intern("PRIMARY", false));
					primary.Text = sb.ToString();
				}
		}
	}
}