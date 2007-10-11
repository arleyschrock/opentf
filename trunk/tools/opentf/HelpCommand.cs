//
// HelpCommand.cs
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
using System.Reflection;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Mono.GetOptions;

[Command("help", "Describe the usage of this program or its subcommands.", "<command>")]
class HelpCommand : Command
{
	[Option("List all commands without description", "L", "list")]
		public bool OptionList = false;

	public HelpCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public void ShowHelp()
	{
		Console.WriteLine("Usage: tf [subcommand] [arguments]");
		Console.WriteLine();
		Console.WriteLine("Available subcommands:");
		Console.WriteLine();

		foreach (CommandAttribute attribute in CommandRegistry.Attributes.Values)
			{
				ShowCommandHelp(attribute, "  ");
			}
	}

	public void ShowList()
	{
		foreach (string attribute in CommandRegistry.Attributes.Keys)
			{
				Console.WriteLine(attribute);
			}
	}

	static public void ShowCommandHelp(CommandAttribute attribute,
																		 string indent)
	{
		Console.Write(indent + attribute.Name);
		if (!String.IsNullOrEmpty(attribute.Alias)) Console.Write(" (alias {0})", attribute.Alias);
		Console.WriteLine();
						
		string description = attribute.Description;
		int x = 0;

		while (x < description.Length)
			{
				int y = Math.Min(x + 60, description.Length);
				int z = description.IndexOf(' ', y) + 1;
				if (z == 0) z = description.Length;

				Console.WriteLine(indent + "  " + description.Substring(x, z - x));
				x = z; 
			}

		Console.WriteLine();
	}

	public void ShowCommandHelp(string cmd)
	{
		CommandAttribute attr = CommandRegistry.GetCommandAttribute(cmd);
		if (attr == null)
			{
				Console.WriteLine("Unknown command: " + cmd);
				Console.WriteLine();
				ShowHelp();
				return;
			}

		ShowCommandHelp(attr, "");
		Console.WriteLine("Usage: " + cmd + " " + attr.Args);
		Console.WriteLine();
		Console.WriteLine("Options:");

		Type commandType = CommandRegistry.GetCommandType(cmd);
		foreach(MemberInfo mi in commandType.GetMembers()) 
			{
				if (mi.MemberType != MemberTypes.Field) continue;

				OptionAttribute[] attribs = mi.GetCustomAttributes(typeof(OptionAttribute), true) as OptionAttribute[];
				if (attribs == null || attribs.Length == 0) continue;

				foreach (OptionAttribute attrib in attribs)
					{
						Type fieldType = ((FieldInfo)mi).FieldType;
						if (!String.IsNullOrEmpty(attrib.AlternateForm))
							{
								string altform = "  /" + attrib.AlternateForm + "";
								if (fieldType != typeof(bool)) altform += ":ARG";
								Console.Write(altform.PadRight(20));
							}

						Console.Write("\t" + attrib.ShortDescription);

						if (!String.IsNullOrEmpty(attrib.LongForm))
							{
								Console.Write(" (also /" + attrib.LongForm);
								if (fieldType != typeof(bool)) Console.Write(":ARG");
								Console.Write(")");
							}

						Console.WriteLine();
					}
			}

		Console.WriteLine();
	}

	public override void Run()
	{
		if (OptionList)
			{
				ShowList();
				return;
			}

		if (Arguments.Length > 0)
			ShowCommandHelp(Arguments[0]);
		else 
			ShowHelp();
	}
}
