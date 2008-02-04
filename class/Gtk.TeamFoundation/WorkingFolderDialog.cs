//
// WorkingFolderDialog.cs
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
using System.Text;
using Gtk;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Gtk.TeamFoundation
{
	public class WorkingFolderDialog : DialogBase
	{
		private string serverPath;
		private Entry localPath;
		private Button browseButton;
		private Button okButton;
		private Button deleteButton;

		public string LocalPath
		{
			get {	return localPath.Text; }
		}

		public string ServerPath
		{
			get { return serverPath; }
		}

		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			if (evnt.Key == Gdk.Key.Return)
				okButton.Click();

			return base.OnKeyPressEvent(evnt);
		}

		public WorkingFolderDialog(Workspace workspace, string serverPath) : base("Working Folder Mapping")
			{
				this.serverPath = serverPath;
				Table table = new Table(3, 3, false);
				table.RowSpacing = ((uint)(6));
				table.ColumnSpacing = ((uint)(6));
				table.BorderWidth = ((uint)(12));

				Label label = new Label("_Server Path: ");
				table.Attach(label, 0, 1, 1, 2);
				Label labelPath = new Label(serverPath);
				labelPath.Xalign = 0;
				table.Attach(labelPath, 1, 2, 1, 2);

				localPath = AddLabeledEntry(table, "_Local Path:", 2, 3);
				localPath.WidthChars = 32;

				browseButton = new Button("Browse...");
				table.Attach(browseButton, 2, 3, 2, 3);

				string lpath = workspace.TryGetLocalItemForServerItem(serverPath);
				if (!String.IsNullOrEmpty(lpath))
					{
						localPath.Text = lpath;
						deleteButton = AddButton("Remove Mapping", ResponseType.Reject) as Button;
					}

				VBox.Add(table);

				okButton = AddButton("OK", ResponseType.Ok) as Button;
				browseButton.Pressed += OnBrowseButtonPressed;

				AddCloseButton("Cancel");
				DefaultResponse = ResponseType.Ok;
				ShowAll();
			}

		void OnBrowseButtonPressed(object sender, EventArgs e)
		{
			Gtk.FileChooserDialog dialog =
				new Gtk.FileChooserDialog("Choose a directory",
																	this,
																	FileChooserAction.CreateFolder,
																	"Cancel",ResponseType.Cancel,
																	"Open",ResponseType.Ok);

			if (dialog.Run() == (int)ResponseType.Ok)
				localPath.Text = dialog.Filename;

			dialog.Destroy();
		}
	}
}