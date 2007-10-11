using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Gtk;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

public class ChangesetDiffView : ScrolledWindow, IChangesetViewChild
{
	private Driver driver;
	private MyTextView textView;

	public ChangesetDiffView(Driver driver)
		{
			this.driver = driver;
			textView = MyTextViewFactory.CreateNewTextView();
			Add(textView);
		}

	protected DiffOptions GetDiffOptions(StreamWriter writer)
	{
		DiffOptions options = new DiffOptions();
		options.UseThirdPartyTool = false;
		options.Flags = DiffOptionFlags.EnablePreambleHandling;
		options.OutputType = DiffOutputType.Unified;
		options.TargetEncoding = Encoding.UTF8;
		options.SourceEncoding = Encoding.UTF8;
		options.StreamWriter = writer;
		options.StreamWriter.AutoFlush = true;

		return options;
	}

	public void Clear()
	{
		textView.Clear();
	}

	public void UpdateCid(int cid)
	{
		ChangesetVersionSpec versionSpec = new ChangesetVersionSpec(cid);

		Toplevel.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);

		string tname = System.IO.Path.GetTempFileName();
		using (StreamWriter sw = new StreamWriter(tname))
			{
				DiffHelper.ShowChangeset(driver.VersionControlServer, versionSpec,
																 false, GetDiffOptions(sw));
			}

		using (StreamReader sr = new StreamReader(tname))
			{ 
				textView.Update("temp.diff", sr.ReadToEnd());
			}

		Toplevel.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.LeftPtr);
		File.Delete(tname);
	}
}
