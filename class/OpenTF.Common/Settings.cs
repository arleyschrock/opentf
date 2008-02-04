//
// Settings.cs
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

namespace OpenTF.Common
{
	public class Settings : SortedList<string, string>
	{
		private readonly string ConfigFile = "TfClient.config";
		private bool initialized = false;
		private static Settings current = new Settings();

		public static Settings Current
		{
			get { return current; }
		}

		private Settings()
			{
			}

		private void InitializeAsNeeded()
		{
			if (initialized) return;
		
			Add("Checkin.Validate", "false");
			Add("Checkout.Latest", "false");
			Add("Credentials.Save", "false");
			Add("File.Excludes", "");
			Add("File.ReadWrite", "false");
			Add("Get.DefaultToCwd", "false");
			Add("Get.ChangesetMtimes", "false");
			Add("Get.Recursive", "false");
			Add("History.DefaultToCwd", "false");
			Add("History.Detailed", "false");
			Add("History.Recursive", "false");
			Add("History.StopAfter", "256");
			Add("Merges.Recursive", "false");
			Add("Online.Recursive", "false");
			Add("Server.Default", "");
			Add("Workspace.Default", "");

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

		public void Save()
		{
			InitializeAsNeeded();

			XmlDocument doc = new XmlDocument();

			XmlElement root = doc.CreateElement("configuration");
			doc.AppendChild(root);

			XmlElement appSettings = doc.CreateElement("appSettings");
			root.AppendChild(appSettings);

			foreach (string key in Keys)
				{
					XmlElement add = doc.CreateElement("add");
					add.SetAttributeNode("key", "").Value = key;
					add.SetAttributeNode("value", "").Value = this[key];
					appSettings.AppendChild(add);
				}

			if (!Directory.Exists(TeamFoundationServer.ClientSettingsDirectory))
				Directory.CreateDirectory(TeamFoundationServer.ClientSettingsDirectory);

			string configFilePath = Path.Combine(TeamFoundationServer.ClientSettingsDirectory, ConfigFile);
			using (XmlTextWriter writer = new XmlTextWriter(configFilePath, null))
				{
					writer.Formatting = Formatting.Indented;
					doc.Save(writer);			
				}
		}

		public string Get(string key)
		{
			InitializeAsNeeded();

			string value = String.Empty;
			TryGetValue(key, out value);

			return value;
		}

		public bool GetAsBool(string key)
		{
			InitializeAsNeeded();

			string value = String.Empty;
			if (TryGetValue(key, out value))
				{
					return Convert.ToBoolean(value);
				}

			return false;
		}

		public int GetAsInt(string key)
		{
			InitializeAsNeeded();

			string value = String.Empty;
			if (TryGetValue(key, out value))
				{
					return Convert.ToInt32(value);
				}

			return -1;
		}
	}
}
