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

public class FileView : Gtk.ScrolledWindow, IExploreViewChild
{
	private MyTextView view;
	private Driver driver;

	public FileView(Driver driver)
		{
			this.driver = driver;
			view = MyTextViewFactory.CreateNewTextView();
			Add(view);
		}

	public void UpdatePath(string path)
	{
		if (String.IsNullOrEmpty(path)) return;
		Microsoft.TeamFoundation.VersionControl.Client.Item item = driver.VersionControlServer.GetItem(path, VersionSpec.Latest, 0, true);

		string tname = System.IO.Path.GetTempFileName();
		item.DownloadFile(tname);
				
		using (StreamReader sr = new StreamReader(tname))
			{
				view.Update(path, sr.ReadToEnd());
			}
	
		File.Delete(tname);
	}
}
