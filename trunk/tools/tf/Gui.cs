//
// gui.cs
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

class AuthDialog : Gtk.Dialog
{
	private Entry username;
	private Entry password;
	private Button okButton;
	private Button cancelButton;

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
		if (evnt.Key == Gdk.Key.Escape)
			cancelButton.Click();

		return base.OnKeyPressEvent(evnt);
	}

	public AuthDialog() : base("Enter Credentials", null, DialogFlags.Modal)
		{
			Label intro = new Label("Please enter your logon credentials.");
			intro.Xpad = 10;
			intro.Ypad = 10;
			intro.SetPadding(10, 10);

			VBox.Add(new Label("Please enter your logon credentials."));
			Table table = new Table(2, 2, false);

			{
				Label label = new Label("_Username:");
				table.Attach(label, 0, 1, 1, 2);
			}

			username = new Entry ();
			username.IsEditable = true;
			table.Attach(username, 1, 2, 1, 2);
		
			{
				Label label = new Label("_Password:");
				table.Attach(label, 0, 1, 2, 3);
			}

			password = new Entry ();
			password.IsEditable = true;
			table.Attach(password, 1, 2, 2, 3);

			VBox.Add(table);

			okButton = AddButton ("OK", ResponseType.Ok) as Button;
			cancelButton = AddButton ("Cancel", ResponseType.Cancel) as Button;

			DefaultResponse = ResponseType.Ok;
		}
}

partial class Driver
{
	public string PromptForLogin(string serverUrl)
	{
		Application.Init();
		AuthDialog dialog = new AuthDialog();

		dialog.ShowAll();
		int rc = dialog.Run();

		string login = dialog.Login;
		dialog.Destroy();

		if (rc != Convert.ToInt32(ResponseType.Ok)) return String.Empty;
		return login;
	}

}
