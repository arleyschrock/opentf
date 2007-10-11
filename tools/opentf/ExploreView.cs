using System;
using Gtk;

public interface IExploreViewChild
{
	void UpdatePath(string path);
}

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

public class ExploreView : Gtk.Window
{
	private RepositoryView repositoryView;
	private ChangesetView changesetView;
	private DirectoryView directoryView;
	private FileView fileView;
	private Notebook viewChildren;
	private string currentSelectedPath;
	
	public ExploreView(Driver driver, int stopAfter) : base ("Explore")
	{
		int x, y, width, height, depth;
		RootWindow.GetGeometry (out x, out y, out width, out height, out depth);
		SetDefaultSize (width - 50, height - 40);

		VBox vBox = new VBox(false, 1);
		Add(vBox);

		HPaned hPaned = new HPaned();
		vBox.Add(hPaned);

		ScrolledWindow scrolledWindow = new ScrolledWindow();
		//scrolledWindow.SetDefaultSize (Convert.ToInt32((width - 50) * 0.4), height -40);
		hPaned.Add1(scrolledWindow);

		repositoryView = new RepositoryView(driver);
		scrolledWindow.Add(repositoryView);

		viewChildren = new Notebook ();

		changesetView = new ChangesetView(driver, stopAfter);
		viewChildren.AppendPage (changesetView, new Label ("Changeset View"));

		directoryView = new DirectoryView(driver);
		viewChildren.AppendPage (directoryView, new Label ("Directory View"));

		fileView = new FileView(driver);
		viewChildren.AppendPage (fileView, new Label ("File View"));

		hPaned.Add2(viewChildren);
		hPaned.Position = (width - 50) / 3;

		// add status bar
		Statusbar sb = new Statusbar ();
		sb.HasResizeGrip = false;
		vBox.PackEnd(sb, false, false, 1);
	
 		ShowAll();

		repositoryView.Selection.Changed += OnPathSelectionChanged;
		viewChildren.SwitchPage += OnSwitchPage;
	}

	void OnPathSelectionChanged (object o, EventArgs args)
	{
		TreeIter iter;
		TreeModel model;
 
		if (!((TreeSelection)o).GetSelected (out model, out iter)) return;

		currentSelectedPath = (string) model.GetValue (iter, 1);

		IExploreViewChild child = viewChildren.CurrentPageWidget as IExploreViewChild;
		UpdateChildPath(child);
	}

	public void OnSwitchPage (object o, SwitchPageArgs args)
	{
		IExploreViewChild child = viewChildren.GetNthPage((int)args.PageNum) as IExploreViewChild;
		UpdateChildPath(child);
	}

	protected void UpdateChildPath(IExploreViewChild child)
	{
		GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
		child.UpdatePath(currentSelectedPath);
		GdkWindow.Cursor = null;
	}

	protected override bool OnDeleteEvent (Gdk.Event ev)
	{
		Application.Quit ();
		return true;
	}
}
