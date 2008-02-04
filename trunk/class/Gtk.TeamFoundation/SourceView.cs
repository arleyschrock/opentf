//
// SourceView.cs
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
using GtkSourceView;
using Pango;
using Gnome.Vfs;

namespace Gtk.TeamFoundation
{
	public class MyTextViewFactory
	{
		static public MyTextView CreateNewTextView()
		{
			SourceLanguage lang = MyTextView.LanguageManager.GetLanguageFromMimeType ("text/x-csharp");
			SourceBuffer buffer = new SourceBuffer(lang);
			buffer.Highlight = true;
			return new MyTextView(buffer);
		}
	}

	public class MyTextView : SourceView
	{
		static public SourceLanguagesManager LanguageManager = new SourceLanguagesManager();
		private SourceBuffer sourceBuffer;

		static MyTextView()
			{
				Vfs.Initialize();
			}

		public MyTextView(SourceBuffer buffer) : base(buffer)
			{
				CursorVisible = false;
				Editable = false;
				ModifyFont(Pango.FontDescription.FromString("Vera Sans Mono 14"));
				ShowLineNumbers = true;

				sourceBuffer = buffer;
			}

		public void Clear()
		{
			Buffer.Text = String.Empty;
		}

		public void Update(string path, string contents)
		{
			string mimeType = Mime.TypeFromNameOrDefault(path, "text/plain");
			//Console.WriteLine("Switching to mimetype: " + mimeType);

			SourceLanguage lang = LanguageManager.GetLanguageFromMimeType (mimeType);

			if (lang != null)
				{
					//Console.WriteLine("Switching to language: " + lang.Name);
					sourceBuffer.Language = lang;
					sourceBuffer.Highlight = true;
				}

			Buffer.Text = contents;
		}
	}
}