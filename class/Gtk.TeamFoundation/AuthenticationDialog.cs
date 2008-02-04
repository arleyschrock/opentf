//
// AuthenticationDialog.cs
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

namespace Gtk.TeamFoundation
{
	public class AuthenticationDialog : DialogBase
	{
		private Entry username;
		private Entry password;
		private Button okButton;

		public string Login
		{
			get {
				return username.Text + "," + password.Text;
			}
		}

		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			if (evnt.Key == Gdk.Key.Return)
				okButton.Click();

			return base.OnKeyPressEvent(evnt);
		}

		public AuthenticationDialog(string uri) : base("Enter Credentials")
			{
				VBox.Add(new Label("Server: " + uri));
				VBox.Add(new Label("Please enter your logon credentials."));
				VBox.Add(new Label(" "));

				Table table = new Table(2, 2, false);
				username = AddLabeledEntry(table, "_Username:", 1, 2);
				password = AddLabeledEntry(table, "_Password:", 2, 3);
				VBox.Add(table);

				okButton = AddButton ("OK", ResponseType.Ok) as Button;
				AddCloseButton("Cancel");

				DefaultResponse = ResponseType.Ok;
			}
	}
}