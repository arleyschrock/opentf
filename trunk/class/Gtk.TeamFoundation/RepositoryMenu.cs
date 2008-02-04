//
// RepositoryMenu.cs
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
using OpenTF.Common;

namespace Gtk.TeamFoundation
{
	public class RepositoryMenu : MenuBase
	{
		//MenuItem
		private MenuItem getLatestItem;
		private MenuItem workingFolderItem;
		private MenuItem addRepoItem;
		private MenuItem editRepoItem;
		private MenuItem deleteRepoItem;
		private Workspace currentWorkspace;
		private string currentPath;
		private ExploreView exploreView;

		public MenuItem GetLatestItem
		{
			get { return getLatestItem; }
		}

		public RepositoryMenu(ExploreView exploreView)
      {
        this.exploreView = exploreView;

				getLatestItem = AddImageMenuItem("Get Latest", Images.Update);
				getLatestItem.Activated += GetLatestHandler;

				workingFolderItem = AddImageMenuItem("Working Folder...", Images.Folder);
				workingFolderItem.Activated += WorkingFolderHandler;

				Append(new SeparatorMenuItem());

				addRepoItem = AddImageMenuItem("Add Repository...", Stock.Add);
				addRepoItem.Activated += AddRepositoryHandler;

				editRepoItem = AddImageMenuItem("Edit Repository...", Stock.Edit);
				editRepoItem.Activated += EditRepositoryHandler;

				deleteRepoItem = AddImageMenuItem("Delete Repository", Stock.Delete);

				ShowAll();
			}

		public void Show(Workspace workspace, string path)
		{
			bool root_folder = (path == VersionControlPath.RootFolder);
			bool path_mapped = (workspace != null && workspace.IsServerPathMapped(path));

			getLatestItem.Sensitive = (path_mapped && !root_folder);
			workingFolderItem.Sensitive = (workspace != null && !root_folder);

			editRepoItem.Sensitive = root_folder;
			deleteRepoItem.Sensitive = root_folder;

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

		public void AddRepositoryHandler(object sender, EventArgs e)
		{
			RepositoryDialog dialog = new RepositoryDialog(null);
			if (dialog.Run() == (int)ResponseType.Ok)
			{
				try
					{
						ICredentials creds = new TFCredential(dialog.Username, dialog.Password);
						TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer(dialog.Server, creds);

						VersionControlServer vcs = tfs.GetService(typeof(VersionControlServer)) as VersionControlServer;
						vcs.CreateWorkspace(dialog.WorkspaceName, dialog.Username, 
																"Created by OpenTF Gui", null, Environment.MachineName);
					}
				catch (WebException ex)
					{
						Console.WriteLine(ex.ToString());
					}
			}

			dialog.Destroy();
		}

		public void EditRepositoryHandler(object sender, EventArgs e)
		{
			RepositoryDialog dialog = new RepositoryDialog(currentWorkspace);

			dialog.Run();
			dialog.Destroy();
		}

		public void WorkingFolderHandler(object sender, EventArgs e)
		{
			WorkingFolderDialog dialog = new WorkingFolderDialog(currentWorkspace, currentPath);

			int rc = dialog.Run();
			if (dialog.Run() == (int)ResponseType.Ok)
			{
				List<WorkingFolder> folders = new List<WorkingFolder>();
				bool found = false;

				foreach (WorkingFolder folder in currentWorkspace.Folders)
					{
						if (folder.ServerItem != dialog.ServerPath)
							{
								folders.Add(folder);
								continue;
							}

						folders.Add(new WorkingFolder(folder.ServerItem, dialog.LocalPath));
						found = true;
					}

				if (!found)
					folders.Add(new WorkingFolder(dialog.ServerPath, dialog.LocalPath));

				currentWorkspace.Update(currentWorkspace.Name, currentWorkspace.Comment, 
																folders.ToArray());
			}
			else if (rc == (int)ResponseType.Reject)
			{
				List<WorkingFolder> folders = new List<WorkingFolder>();

				foreach (WorkingFolder folder in currentWorkspace.Folders)
					{
						if (folder.ServerItem == dialog.ServerPath) continue;
						folders.Add(folder);
					}

				currentWorkspace.Update(currentWorkspace.Name, currentWorkspace.Comment, 
																folders.ToArray());
			}

			dialog.Destroy();
		}
	}
}
