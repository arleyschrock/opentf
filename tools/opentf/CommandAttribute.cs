//
// CommandAttribute.cs
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

[AttributeUsage(AttributeTargets.Class)]
public class CommandAttribute : Attribute
{
	private string name;
	private string description;
	private string alias;
	private string args;
	
	public CommandAttribute(string name, string description)
		{
			this.name = name;
			this.description = description;
		}

	public CommandAttribute(string name, string description, string args)
		{
			this.name = name;
			this.description = description;
			this.args = args;
		}

	public CommandAttribute(string name, string description, string args, string alias)
		{
			this.name = name;
			this.description = description;
			this.alias = alias;
			this.args = args;
		}

	public string Name
	{
		get { return name; }
	}

	public string Alias
	{
		get { return alias; }
	}

	public string Args
	{
		get { return args; }
	}

	public string Description
	{
		get { return description; }
	}

}
