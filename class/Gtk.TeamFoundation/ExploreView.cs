using System;
using Gtk;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Gtk.TeamFoundation
{
	public interface IExploreViewChild
	{
		void UpdatePath(VersionControlServer vcs, string path);
	}

	public class ExploreView : Gtk.VBox
	{
		private RepositoryView repositoryView;
		private ChangesetView changesetView;
		private DirectoryView directoryView;
		private FileView fileView;
		private Notebook viewChildren;
		private string currentSelectedPath;
		private VersionControlServer currentVcs;
		private IVersionControlServerFactory vcsFactory;

		public ExploreView(IVersionControlServerFactory vcsFactory, 
											 int stopAfter) : base (false, 1)
			{
				this.vcsFactory = vcsFactory;

				HPaned hPaned = new HPaned();
				Add(hPaned);

				ScrolledWindow scrolledWindow = new ScrolledWindow();
				hPaned.Add1(scrolledWindow);

				repositoryView = new RepositoryView(vcsFactory);
				scrolledWindow.Add(repositoryView);

				viewChildren = new Notebook();

				changesetView = new ChangesetView(stopAfter);
				viewChildren.AppendPage(changesetView, new Label ("Changeset View"));

				directoryView = new DirectoryView();
				viewChildren.AppendPage(directoryView, new Label ("Directory View"));

				fileView = new FileView();
				viewChildren.AppendPage(fileView, new Label ("File View"));

				int x, y, width, height, depth;
				RootWindow.GetGeometry (out x, out y, out width, out height, out depth);

				hPaned.Add2(viewChildren);
				hPaned.Position = (width - 50) / 3;

				// add status bar
				Statusbar sb = new Statusbar ();
				sb.HasResizeGrip = false;
				PackEnd(sb, false, false, 1);
	
				ShowAll();

				repositoryView.Selection.Changed += OnPathSelectionChanged;
				viewChildren.SwitchPage += OnSwitchPage;
			}

		void OnPathSelectionChanged (object o, EventArgs args)
		{
			TreeIter iter;
			TreeModel model;
 
			if (!((TreeSelection)o).GetSelected (out model, out iter)) return;

			TreeIter iterParent;
			if (model.IterParent(out iterParent, iter))
				{
					string url = (string) model.GetValue (iterParent, 0);
					currentVcs = vcsFactory.GetVersionControlServer(url);

					currentSelectedPath = (string) model.GetValue (iter, 1);
					IExploreViewChild child = viewChildren.CurrentPageWidget as IExploreViewChild;
					UpdateChildPath(child);
				}
		}

		public void OnSwitchPage (object o, SwitchPageArgs args)
		{
			IExploreViewChild child = viewChildren.GetNthPage((int)args.PageNum) as IExploreViewChild;
			UpdateChildPath(child);
		}

		protected void UpdateChildPath(IExploreViewChild child)
		{
			GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
			child.UpdatePath(currentVcs, currentSelectedPath);
			GdkWindow.Cursor = null;
		}

	}
}