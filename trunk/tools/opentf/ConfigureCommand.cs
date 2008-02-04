//
// ConfigureCommand.cs
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
using System.Xml;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using OpenTF.Common;

[Command("configure", "Display/Update current configuration.", "<key> <value>", "config")]
class ConfigureCommand : Command
{
	public ConfigureCommand(Driver driver, string[] args): base(driver, args)
		{
		}

	private void ShowConfiguration()
	{
		Console.WriteLine("Client Configuration:");
		StringBuilder sb = new StringBuilder();
		foreach (string key in Settings.Current.Keys)
			{
				string value = Settings.Current[key];
				if (String.IsNullOrEmpty(value)) value = "<empty>";

				string line = String.Format("  {0}:\t{1}\n", key, value);
				if (key.StartsWith("http")) sb.Append(line);
				else Console.Write(line);
			}

		string creds = sb.ToString();
		if (creds.Length > 0)
			{
				Console.WriteLine();
				Console.WriteLine("Credentials:");
				Console.WriteLine(creds);
			}

		Console.WriteLine("Configuration File: " + Settings.Current.GetConfigPath());
	}

	public override void Run()
	{
		if (Arguments.Length == 1)
			{
				string key = Arguments[0];
				Settings.Current.Remove(key);
				Settings.Current.Save();
			}
		else if (Arguments.Length == 2)
			{
				string key = Arguments[0];
				string value = Arguments[1];
				Settings.Current[key] = value;
				Settings.Current.Save();
			}

		ShowConfiguration();
	}
}

