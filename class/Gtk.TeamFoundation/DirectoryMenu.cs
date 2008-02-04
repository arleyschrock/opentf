//
// DirectoryMenu.cs
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
using System.IO;
using System.Net;
using Gtk;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Gtk.TeamFoundation
{
	public class DirectoryMenu : MenuBase
	{
		private ExploreView exploreView;
		private MenuItem getLatestItem;
		private Workspace currentWorkspace;
		private string currentPath;

		public MenuItem GetLatestItem
		{
			get { return getLatestItem; }
		}

		public DirectoryMenu(ExploreView exploreView)
			{
				this.exploreView = exploreView;

				getLatestItem = AddImageMenuItem("Get Latest", Images.Update);
				getLatestItem.Activated += GetLatestHandler;

				ShowAll();
			}

		public void Show(Workspace workspace, string path)
		{
			bool root_folder = (path == VersionControlPath.RootFolder);
			bool path_mapped = workspace.IsServerPathMapped(path);

			getLatestItem.Sensitive = (path_mapped && !root_folder);

			currentWorkspace = workspace;
			currentPath = path;

			Popup();
		}

		public void GetLatestHandler(object sender, EventArgs e)
		{
			List<GetRequest> requests = new List<GetRequest>();

			string lpath = currentWorkspace.GetLocalItemForServerItem(currentPath);
			requests.Add(new GetRequest(System.IO.Path.GetFullPath(lpath), RecursionType.Full, 
																	VersionSpec.Latest));

			exploreView.GetFromRepository(currentWorkspace, requests.ToArray());
		}

	}
}
