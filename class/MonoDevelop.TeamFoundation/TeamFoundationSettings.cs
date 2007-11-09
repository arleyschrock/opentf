//
// TeamFoundationSettings.cs
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

namespace MonoDevelop.TeamFoundation
{
	public class TeamFoundationSettings : SortedList<string, string>
	{
		private readonly string ConfigFile = "TfClient.config";
		private bool initialized = false;
		private static TeamFoundationSettings current = new TeamFoundationSettings();

		public static TeamFoundationSettings Current
		{
			get { return current; }
		}

		private TeamFoundationSettings()
			{
			}

		private void InitializeAsNeeded()
		{
			if (initialized) return;
			initialized = true;

			string configFilePath = GetConfigPath();
			if (!File.Exists(configFilePath)) return;

			using (XmlTextReader reader = new XmlTextReader(configFilePath))
				{
					while (reader.Read())
						{
							if (reader.NodeType == XmlNodeType.Element && reader.Name == "add")
								{
									string key = reader.GetAttribute("key");
									string value = reader.GetAttribute("value");
									this[key] = value;
								}
						}
				}
		}

		public string GetConfigPath()
		{
			return Path.Combine(TeamFoundationServer.ClientSettingsDirectory, ConfigFile); 
		}

		public string Get(string key)
		{
			InitializeAsNeeded();

			string value = String.Empty;
			TryGetValue(key, out value);

			return value;
		}
	}
}