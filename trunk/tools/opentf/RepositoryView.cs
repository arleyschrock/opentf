using System;
using System.Net;
using Gtk;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

public class RepositoryView : Gtk.TreeView
{
	private Gtk.TreeStore itemStore;
	private Driver driver;
	
	private Gtk.TreeIter SetRowValue(Gtk.TreeIter parent, int childIndx,
																	 string cell1, string cell2)
	{
		Gtk.TreeIter child;
		if (itemStore.IterNthChild(out child, parent, childIndx))
			{
				itemStore.SetValue(child, 0, cell1);
				itemStore.SetValue(child, 1, cell2);
			}
		else
			child = itemStore.AppendValues (parent, cell1, cell2);

		return child;
	}

	private void MyRowExpandedHandler (object o, RowExpandedArgs args)
	{
		string path = itemStore.GetValue(args.Iter, 1).ToString() + "/*";
		//Console.WriteLine(path);

		int indx = 0;
		Microsoft.TeamFoundation.VersionControl.Client.Item[] items = ItemsForPath(path);
		if (items.Length == 0)
			SetRowValue(args.Iter, indx, " - item list not available - ", "");

		foreach (Microsoft.TeamFoundation.VersionControl.Client.Item item in items)
			{
				string shortPath = item.ServerItem.Substring(item.ServerItem.LastIndexOf('/') + 1);
				if (item.ItemType == ItemType.Folder)
					shortPath += "/";

				Gtk.TreeIter child = SetRowValue(args.Iter, indx, shortPath, item.ServerItem);

				if (item.ItemType == ItemType.Folder)
					itemStore.AppendValues(child, "", "");

				indx++;
			}
	}

	private Microsoft.TeamFoundation.VersionControl.Client.Item[] ItemsForPath(string path)
	{
		ItemSpec itemSpec = new ItemSpec (path, RecursionType.OneLevel);
		ItemSet itemSet = driver.VersionControlServer.GetItems (itemSpec, VersionSpec.Latest, 
																														DeletedState.NonDeleted, ItemType.Any, false);

		return itemSet.Items;
	}

	public RepositoryView(Driver driver)
		{
			this.driver = driver;
			HeadersVisible = false;

			AppendColumn ("", new Gtk.CellRendererText (), "text", 0);
			TreeViewColumn fullPath = AppendColumn ("", new Gtk.CellRendererText (), "text", 0);
			fullPath.Visible = false;

			itemStore = new Gtk.TreeStore (typeof (string), typeof (string));
			Gtk.TreeIter root = itemStore.AppendValues(VersionControlPath.RootFolder, VersionControlPath.RootFolder);

			Microsoft.TeamFoundation.VersionControl.Client.Item[] items = ItemsForPath("$/");
			foreach (Microsoft.TeamFoundation.VersionControl.Client.Item item in items)
			{
				if (item.ServerItem == VersionControlPath.RootFolder) continue;

				string shortPath = item.ServerItem.Substring(item.ServerItem.LastIndexOf('/') + 1) + "/";
				Gtk.TreeIter iter = itemStore.AppendValues (root, shortPath, item.ServerItem);

				itemStore.AppendValues(iter, "");
			}

			Model = itemStore;

			ExpandRow(TreePath.NewFirst(), false);
			RowExpanded += MyRowExpandedHandler;
			KeyReleaseEvent += MyKeyReleaseEventHandler;
		}

	public void MyKeyReleaseEventHandler (object o, KeyReleaseEventArgs args)
	{
		if ((Gdk.Key.c == args.Event.Key) && ((args.Event.State & Gdk.ModifierType.ControlMask) != 0))
			{
				TreeIter iter;
				TreeModel model;
				if (!(Selection.GetSelected (out model, out iter))) return;

				string path = model.GetValue(iter, 1).ToString();
				Clipboard primary = Clipboard.Get(Gdk.Atom.Intern ("PRIMARY", false));
				primary.Text = path;
			}
	}
}
