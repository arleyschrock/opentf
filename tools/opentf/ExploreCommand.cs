//
// ExploreCommand.cs
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
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Mono.GetOptions;
using Gtk;
using Gtk.TeamFoundation;

[Command("explore", "Explore the repository visually.", "", "gui")]
class ExploreCommand : Command
{
	[Option("Version", "V", "version")]
		public string OptionVersion;

	[Option("Limit the number of changesets shown (default=100)", "", "stopafter")]
		public int OptionStopAfter = 100;

	private ExploreView exploreView;
	
	public ExploreCommand(Driver driver, string[] args): base(driver, args)
		{
		}

	public override void Run()
	{
		Application.Init();

		Gtk.Window frame = new Gtk.Window("OpenTF Explorer");

		exploreView = new ExploreView(Driver, OptionStopAfter);
		exploreView.ShowChangeset += MyShowChangesetEventHandler;
		exploreView.ShowFile += MyShowFileEventHandler;

		frame.Add(exploreView);
		frame.DeleteEvent += new DeleteEventHandler(DeleteEvent);
		frame.KeyReleaseEvent += MyKeyReleaseHandler;

		int x, y, width, height, depth;
		frame.RootWindow.GetGeometry (out x, out y, out width, out height, out depth);
		frame.SetDefaultSize(Convert.ToInt32(width*.9), Convert.ToInt32(height*.9));

		frame.ShowAll();
		Application.Run();
	}

	void MyShowChangesetEventHandler(object sender, ShowChangesetEventArgs args)
	{
		ShowChangesetDialog dialog = new ShowChangesetDialog(args.VersionControlServer, args.ChangesetId);

		int x, y, width, height, depth;
		Gtk.Widget widget = sender as Gtk.Widget;
		widget.RootWindow.GetGeometry (out x, out y, out width, out height, out depth);
		dialog.SetDefaultSize(Convert.ToInt32(width*.9), Convert.ToInt32(height*.9));

		dialog.ShowAll();
		dialog.Run();
		dialog.Destroy();
	}

	void MyShowFileEventHandler(object sender, ShowFileEventArgs args)
	{
		ShowFileDialog dialog = new ShowFileDialog(args.VersionControlServer, args.ServerItem);

		int x, y, width, height, depth;
		Gtk.Widget widget = sender as Gtk.Widget;
		widget.RootWindow.GetGeometry(out x, out y, out width, out height, out depth);
		dialog.SetDefaultSize(Convert.ToInt32(width*.9), Convert.ToInt32(height*.9));

		dialog.ShowAll();
		dialog.Run();
		dialog.Destroy();
	}

	static void DeleteEvent(object obj, DeleteEventArgs args)
	{
		Application.Quit();
	}

	void MyKeyReleaseHandler(object o, KeyReleaseEventArgs args)
	{
		if ((Gdk.Key.q == args.Event.Key) && ((args.Event.State & Gdk.ModifierType.ControlMask) != 0))
			Application.Quit();
	}


}
