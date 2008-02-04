//
// GettingDialog.cs
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
using System.Text;
using Gtk;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using OpenTF.Common;

namespace Gtk.TeamFoundation
{
	public class GettingDialog : DialogBase
	{
		private List<string> getLatestList = new List<string>();
		private ProgressBar progressBar;
		private Label fileLabel;

		public void Pulse(string msg)
		{
			Console.WriteLine(msg);
			fileLabel.Text = msg;
			progressBar.Pulse();
		}

		public GettingDialog(VersionControlServer vcs, Workspace workspace, GetRequest[] requests) : base("Progress")
			{
				VBox.Spacing = 10;
				VBox.Add(new Label("Getting files from the server..."));

				progressBar = new ProgressBar();
				VBox.Add(progressBar);

				fileLabel = new Label("");
				VBox.Add(fileLabel);

				AddCloseButton("Cancel");
				DefaultResponse = ResponseType.Cancel;

				ShowAll();

				getLatestList.Clear();
				vcs.Getting += MyGettingEventHandler;

				GetStatus status = workspace.Get(requests, GetOptions.GetAll|GetOptions.Overwrite);
				foreach (string file in getLatestList)
					{
						Console.WriteLine(file);
						Pulse("Setting permissions: " + file);
						if (! FileTypeDatabase.ShouldBeExecutable(file)) continue;
						FileType.MakeExecutable(file);
					}
			}

		public void MyGettingEventHandler(object sender, GettingEventArgs e)
		{
			string msg = String.Empty;
			if (e.DeletionId != 0)
				msg = String.Format("deleting {0}", e.SourceLocalItem);
			else
				{
					if ((!String.IsNullOrEmpty(e.TargetLocalItem))&&
							(!String.IsNullOrEmpty(e.SourceLocalItem))&&
							(e.SourceLocalItem != e.TargetLocalItem))
						msg = String.Format("renaming {0}", e.TargetLocalItem);
					else
						msg = String.Format("updating {0}", e.TargetLocalItem);
				}

			Pulse(msg);

			if (e.ItemType == ItemType.Folder) return;
			getLatestList.Add(e.TargetLocalItem);
		}	
	}
}