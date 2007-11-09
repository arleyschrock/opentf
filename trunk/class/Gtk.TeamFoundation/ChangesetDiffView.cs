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

namespace Gtk.TeamFoundation
{
	public class ChangesetDiffView : ScrolledWindow, IChangesetViewChild
	{
		private IVersionControlServerFactory vcsFactory;
		private MyTextView textView;

		public ChangesetDiffView()
			{
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

		public void UpdateCid(VersionControlServer vcs, int cid)
		{
			ChangesetVersionSpec versionSpec = new ChangesetVersionSpec(cid);

			Toplevel.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);

			string tname = System.IO.Path.GetTempFileName();
			using (StreamWriter sw = new StreamWriter(tname))
				{
					DiffHelper.ShowChangeset(vcs, versionSpec, false, GetDiffOptions(sw));
				}

			using (StreamReader sr = new StreamReader(tname))
				{ 
					textView.Update("temp.diff", sr.ReadToEnd());
				}

			Toplevel.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.LeftPtr);
			File.Delete(tname);
		}
	}
}