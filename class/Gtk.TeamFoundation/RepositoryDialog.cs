//
// RepositoryDialog.cs
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
using OpenTF.Common;

namespace Gtk.TeamFoundation
{
	public class RepositoryDialog : DialogBase
	{
		private Entry name;
		private Entry server;
		private Entry username;
		private Entry password;
		private Button okButton;

		public string WorkspaceName
		{
			get { return name.Text; }
		}

		public string Server
		{
			get { return server.Text; }
		}

		public string Username
		{
			get { return username.Text; }
		}

		public string Password
		{
			get { return password.Text; }
		}

		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			if (evnt.Key == Gdk.Key.Return)
				okButton.Click();

			return base.OnKeyPressEvent(evnt);
		}

		public RepositoryDialog(Workspace workspace) : base((workspace == null)? "Add Repository" : "Edit Repository")
			{
				Table table = new Table(3, 2, false);
				table.RowSpacing = ((uint)(6));
				table.ColumnSpacing = ((uint)(6));
				table.BorderWidth = ((uint)(12));

				name = AddLabeledEntry(table, "_Name:", 1, 2);
				server = AddLabeledEntry(table, "_Server:", 2, 3);
				table.Attach(new Gtk.HSeparator(), 0, 2, 3, 4);

				username = AddLabeledEntry(table, "_Username:", 4, 5);
				password = AddLabeledEntry(table, "_Password:", 5, 6);
				VBox.Add(table);

				okButton = AddButton ("OK", ResponseType.Ok) as Button;
				AddCloseButton("Cancel");
				DefaultResponse = ResponseType.Ok;

				if (workspace != null)
					{
						name.Text = workspace.Name;
						Uri uri = workspace.VersionControlServer.TeamFoundationServer.Uri;
						server.Text = uri.Host.ToString();

						string creds = Keyring.GetCredentials(uri.ToString());
						int comma = creds.IndexOf(",");

						if (comma != -1)
							{
								username.Text = creds.Substring(0, comma);
								password.Text = creds.Substring(comma+1);
							}
						else username.Text = creds;
					}
				else
					{
						name.Text = "CodePlex";
						server.Text = "https://tfs01.codeplex.com";
						username.Text = "snd\\";
					}

				ShowAll();
			}
	}
}