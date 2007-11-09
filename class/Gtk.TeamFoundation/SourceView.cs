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