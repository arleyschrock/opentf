using System;
using System.IO;
using System.Net;
using Gtk;

using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Components.Commands;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Gtk.TeamFoundation;

namespace MonoDevelop.TeamFoundation
{
	public class TeamFoundationPad : AbstractPadContent, IVersionControlServerFactory
	{
		CredentialCache credentialCache = new CredentialCache();
		ExploreView exploreView;

		public TeamFoundationPad() : base ("Team Foundation Browser")
			{
				exploreView = new ExploreView(this, 50);
				exploreView.ShowAll();
			}

		public override Gtk.Widget Control {
			get { return exploreView; }
		}

		public VersionControlServer GetVersionControlServer(string url)
		{
			ICredentials creds = GetCredentials(url);
			TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer(url, creds);
			return tfs.GetService(typeof(VersionControlServer)) as VersionControlServer;
		}

		protected NetworkCredential GetCredentials(string url)
		{
			Uri uri = new Uri(url);
			NetworkCredential creds = credentialCache.GetCredential(uri, "NTLM");
			if (creds != null) return creds;

			string login = TeamFoundationSettings.Current.Get(url);
			string userinfo = String.Empty;

			string username = String.Empty;
			string domain = String.Empty;
			string password = String.Empty;

			int comma = login.IndexOf(",");
			if (comma != -1)
				{
					userinfo = login.Substring(0, comma);
					password = login.Substring(comma+1);
				}
			else userinfo = login;

			// try to find domain portion if given
			int slash = userinfo.IndexOf('\\');
			if (-1 != slash)
				{
					domain = userinfo.Substring(0, slash);
					username = userinfo.Substring(slash+1);	
				}
			else
				{
					int atsign = userinfo.IndexOf('@');
					if (-1 != atsign)
						{
							username = userinfo.Substring(0, atsign);	
							domain = userinfo.Substring(atsign+1);
						}
					else username = userinfo;
				}

			creds = new NetworkCredential(username, password, domain);
			credentialCache.Add(uri, "NTLM", creds);

			return creds;
		}

	}
}