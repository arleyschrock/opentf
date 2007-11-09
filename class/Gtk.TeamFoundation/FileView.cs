using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Gtk;
using Pango;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Gtk.TeamFoundation
{
	public class FileView : Gtk.ScrolledWindow, IExploreViewChild
	{
		private MyTextView view;

		public FileView()
			{
				view = MyTextViewFactory.CreateNewTextView();
				Add(view);
			}

		public void UpdatePath(VersionControlServer vcs, string path)
		{
			if (String.IsNullOrEmpty(path)) return;
			Microsoft.TeamFoundation.VersionControl.Client.Item item = vcs.GetItem(path, VersionSpec.Latest, 0, true);

			string tname = System.IO.Path.GetTempFileName();
			item.DownloadFile(tname);
				
			using (StreamReader sr = new StreamReader(tname))
				{
					view.Update(path, sr.ReadToEnd());
				}
	
			File.Delete(tname);
		}
	}
}