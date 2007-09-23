//
// CommandRegistry.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

public class CommandRegistry
{
	static SortedList<string, System.Type> commands = new SortedList<string, System.Type>();
	static SortedList<string, CommandAttribute> commandAttributes = new SortedList<string, CommandAttribute>();

	static public SortedList<string, CommandAttribute> Attributes
	{
		get { return commandAttributes; }
	}

	static CommandRegistry()
	{
		Assembly assembly = Assembly.GetAssembly(typeof(CommandRegistry));
		System.Type[] types = assembly.GetTypes();

		foreach (Type t in types)
			{
				CommandAttribute[] attributes = t.GetCustomAttributes(typeof(CommandAttribute), false) as CommandAttribute[];
				foreach (CommandAttribute attribute in attributes)
					{
						commands.Add(attribute.Name, t);
						commandAttributes.Add(attribute.Name, attribute);

						if (!String.IsNullOrEmpty(attribute.Alias))
							{
								commands.Add(attribute.Alias, t);
								commandAttributes.Add(attribute.Alias, attribute);
							}
					}
			}
	}

	static public System.Type GetCommandType(string name)
	{
		Type commandType;
		if (!commands.TryGetValue(name, out commandType)) return null;
		return commandType;
	}

	static public CommandAttribute GetCommandAttribute(string name)
	{
		CommandAttribute attribute;
		if (!commandAttributes.TryGetValue(name, out attribute)) return null;
		return attribute;
	}
}

