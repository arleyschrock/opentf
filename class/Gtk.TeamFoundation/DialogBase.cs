//
// DialogBase.cs
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
	public class DialogBase : Gtk.Dialog
	{
		private Button closeButton;

		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			if (evnt.Key == Gdk.Key.Escape && closeButton != null)
				closeButton.Click();

			return base.OnKeyPressEvent(evnt);
		}

		public DialogBase(string title) : base(title, null, DialogFlags.Modal)
		{
		}

		public void AddCloseButton()
		{
			AddCloseButton("Close");
		}

		public void AddCloseButton(string label)
		{
			closeButton = AddButton (label, ResponseType.Cancel) as Button;
			DefaultResponse = ResponseType.Cancel;
		}

		public Entry AddLabeledEntry(Table table, string title, uint top_attach, uint bottom_attach)
		{
			Label label = new Label(title);
			table.Attach(label, 0, 1, top_attach, bottom_attach);

			Entry entry = new Entry();
			entry.IsEditable = true;
			table.Attach(entry, 1, 2, top_attach, bottom_attach);
			
			return entry;
		}

	}
}