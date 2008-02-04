//
// ChangesetDiffView.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Gtk;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using OpenTF.Common;

namespace Gtk.TeamFoundation
{
	public class ChangesetDiffView : ScrolledWindowBase
	{
		private MyTextView textView;

		public ChangesetDiffView(VersionControlServer vcs, int cid)
			{
				textView = MyTextViewFactory.CreateNewTextView();
				Add(textView);
				ShowChangeset(vcs, cid);
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

		public void ShowChangeset(VersionControlServer vcs, int cid)
		{
			ChangesetVersionSpec versionSpec = new ChangesetVersionSpec(cid);

			string tname = System.IO.Path.GetTempFileName();
			using (StreamWriter sw = new StreamWriter(tname))
				{
					DiffHelper.ShowChangeset(vcs, versionSpec, false, GetDiffOptions(sw));
				}

			using (StreamReader sr = new StreamReader(tname))
				{ 
					textView.Update("temp.diff", sr.ReadToEnd());
				}

			File.Delete(tname);
		}
	}
}