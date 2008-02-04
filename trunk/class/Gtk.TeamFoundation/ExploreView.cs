//
// ExploreView.cs
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
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Gtk;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using OpenTF.Common;

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
		private VPaned viewChildren;
		private string currentSelectedPath;
		private VersionControlServer currentVcs;
		private ICredentialsProvider credentialsProvider;
		private Statusbar statusbar;
		private static uint messageIndx;

		public event ShowChangesetEventHandler ShowChangeset;
		public event ShowFileEventHandler ShowFile;

		public ExploreView(ICredentialsProvider credentialsProvider,
											 int stopAfter) : base (false, 1)
			{
				this.credentialsProvider = credentialsProvider;

				HPaned hPaned = new HPaned();
				Add(hPaned);

				{
					ScrolledWindowBase scrolledWindow1 = new ScrolledWindowBase();
					hPaned.Add1(scrolledWindow1);

					repositoryView = new RepositoryView(this, credentialsProvider);
					scrolledWindow1.Add(repositoryView);
				}

				viewChildren = new VPaned();
				{
					ScrolledWindowBase scrolledWindow2 = new ScrolledWindowBase();
					viewChildren.Pack1(scrolledWindow2, true, true);

					directoryView = new DirectoryView(this);
					scrolledWindow2.Add(directoryView);

					ScrolledWindowBase scrolledWindow3 = new ScrolledWindowBase();
					viewChildren.Pack2(scrolledWindow3, true, true);

					changesetView = new ChangesetView(this, stopAfter);
					scrolledWindow3.Add(changesetView);

					int x, y, width, height, depth;
					RootWindow.GetGeometry (out x, out y, out width, out height, out depth);

					hPaned.Add2(viewChildren);
					hPaned.Position = (width - 50) / 3;
				}

				// add status bar
				statusbar = new Statusbar ();
				statusbar.HasResizeGrip = false;
				PackEnd(statusbar, false, false, 1);

				Assembly entry = Assembly.GetEntryAssembly();
				StatusMessage(String.Format("{0} version {1}", entry.GetName().Name,
																		entry.GetName().Version.ToString()));

				ShowAll();
				repositoryView.Selection.Changed += OnPathSelectionChanged;
			}
		
		public void StatusMessage(string msg)
		{
			statusbar.Push(messageIndx++, msg);			
		}

		public void GetFromRepository(Workspace workspace, GetRequest[] requests)
		{
			using (GettingDialog gettingDialog = new GettingDialog(currentVcs, workspace, requests))
				{
					gettingDialog.ShowAll();
					gettingDialog.Run();
					gettingDialog.Destroy();
				}
		}

		public void OnShowChangeset(object sender, ShowChangesetEventArgs args)
		{
			if (ShowChangeset != null) ShowChangeset(sender, args);
		}

		public void OnShowFile(object sender, ShowFileEventArgs args)
		{
			if (ShowFile != null)	ShowFile(sender, args);
		}

		void OnPathSelectionChanged (object o, EventArgs args)
		{
			TreeIter iter;
			TreeModel model;
 
			if (!((TreeSelection)o).GetSelected (out model, out iter)) return;

			TreeIter iterParent; 
			bool not_root = false;
			TreeIter current = iter;
			while (model.IterParent(out iterParent, current))
				{
					current = iterParent;
					not_root = true;
				}

			if (not_root)
				{
					string url = (string) model.GetValue (current, RepositoryView.ColumnIndex.Url);
					ICredentials credentials = credentialsProvider.GetCredentials(new System.Uri(url), null);
					TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer(url, credentials);

					currentVcs = tfs.GetService(typeof(VersionControlServer)) as VersionControlServer;
					if (currentVcs == null) return;

					currentSelectedPath = (string) model.GetValue(iter, RepositoryView.ColumnIndex.Path);

					GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
					directoryView.UpdatePath(currentVcs, currentSelectedPath);
					changesetView.UpdatePath(currentVcs, currentSelectedPath);
					GdkWindow.Cursor = null;
				}
		}
	}
}