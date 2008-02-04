//
// Images.cs
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
using System.IO;
using System.Net;
using Gtk;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Gtk.TeamFoundation
{
	public static class Images
	{
		static private Gdk.Pixbuf repository;
		static private Gdk.Pixbuf folder;
		static private Gdk.Pixbuf file;
		static private Gdk.Pixbuf update;

		static Images()
			{
				repository = Gdk.Pixbuf.LoadFromResource("repository.png");
				folder = IconTheme.Default.LoadIcon("folder", 16, (Gtk.IconLookupFlags)0);
				file = IconTheme.Default.LoadIcon("gnome-fs-regular", 16, (Gtk.IconLookupFlags)0);
				update = Gdk.Pixbuf.LoadFromResource("update.png");
			}

		public static Gdk.Pixbuf Repository { get { return repository; } }
		public static Gdk.Pixbuf Folder { get { return folder; } }
		public static Gdk.Pixbuf File { get { return file; } }
		public static Gdk.Pixbuf Update { get { return update; } }
	}
}