using System;
using System.Text;
using Gtk;
using Pango;

public class MyTextViewFactory
{
	static public MyTextView CreateNewTextView()
	{
		return new MyTextView();
	}
}

public class MyTextView : Gtk.TextView
{
	public MyTextView()
		{
			CursorVisible = false;
			Editable = false;
			ModifyFont(Pango.FontDescription.FromString("Vera Sans Mono 16"));
		}

	public void Clear()
	{
		Buffer.Text = String.Empty;
	}

	public void Update(string path, string contents)
		{
			Buffer.Text = contents;
		}
}
