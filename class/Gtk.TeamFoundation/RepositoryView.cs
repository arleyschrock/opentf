using System;
using System.IO;
using System.Net;
using Gtk;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Gtk.TeamFoundation
{
	public class RepositoryView : Gtk.TreeView
	{
		TreeStore store = new TreeStore(typeof(string), typeof(string));
		ScrolledWindow scroller = new ScrolledWindow();
		CredentialCache credentialCache = new CredentialCache();
		IVersionControlServerFactory vcsFactory;

		public RepositoryView(IVersionControlServerFactory vcsFactory)
			{
				this.vcsFactory = vcsFactory;

				AppendColumn("name", new CellRendererText (), "text", 0);

				WorkspaceInfo[] infos = Workstation.Current.GetAllLocalWorkspaceInfo();
				foreach (WorkspaceInfo info in infos)
					{
						Gtk.TreeIter serverIter = store.AppendValues(info.ServerUri.ToString(), VersionControlPath.RootFolder);
						store.AppendValues(serverIter, "");
					}

				Model = store;
				HeadersVisible = false;
			
				ShowAll();
				RowExpanded += MyRowExpandedHandler;
			}

		private void MyRowExpandedHandler (object o, RowExpandedArgs args)
		{
			string path = store.GetValue(args.Iter, 1).ToString();

			string url = String.Empty;
			TreeIter iterParent;
			if (store.IterParent(out iterParent, args.Iter))
				url = (string) store.GetValue (iterParent, 0);
			else
				url = store.GetValue(args.Iter, 0).ToString();

			VersionControlServer versionControlServer = vcsFactory.GetVersionControlServer(url);

			int indx = 0;
			ItemSpec itemSpec = new ItemSpec (path, RecursionType.OneLevel);
			ItemSet itemSet = versionControlServer.GetItems (itemSpec, VersionSpec.Latest, 
																											 DeletedState.NonDeleted, ItemType.Any, false);
			if (itemSet.Items.Length == 0)
				SetRowValue(args.Iter, indx, " - item list not available - ", "");

			foreach (Microsoft.TeamFoundation.VersionControl.Client.Item item in itemSet.Items)
				{
					if (item.ServerItem == VersionControlPath.RootFolder) continue;

					string shortPath = item.ServerItem.Substring(item.ServerItem.LastIndexOf('/') + 1);
					if (item.ItemType == ItemType.Folder)
						shortPath += "/";

					Gtk.TreeIter child = SetRowValue(args.Iter, indx, shortPath, item.ServerItem);

					if (item.ItemType == ItemType.Folder)
						store.AppendValues(child, "", "");

					indx++;
				}
		}

		private Gtk.TreeIter SetRowValue(Gtk.TreeIter parent, int childIndx,
																		 string cell1, string cell2)
		{
			Gtk.TreeIter child;
			if (store.IterNthChild(out child, parent, childIndx))
				{
					store.SetValue(child, 0, cell1);
					store.SetValue(child, 1, cell2);
				}
			else
				child = store.AppendValues (parent, cell1, cell2);

			return child;
		}

	}
}