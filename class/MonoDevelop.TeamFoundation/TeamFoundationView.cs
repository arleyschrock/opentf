using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Gtk;

using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Components.Commands;
using MonoDevelop.VersionControl.Views;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Gtk.TeamFoundation;
using OpenTF.Common;

namespace MonoDevelop.TeamFoundation
{
	public class TeamFoundationView : AbstractViewContent, ICredentialsProvider
	{
		CredentialCache credentialCache = new CredentialCache();
		ExploreView exploreView;

		public TeamFoundationView() : base ()
			{
				this.ContentName = "Team Foundation Browser";
				this.IsViewOnly = true;

				exploreView = new ExploreView(this, 50);
				exploreView.ShowChangeset += MyShowChangesetEventHandler;
				exploreView.ShowFile += MyShowFileEventHandler;

				exploreView.ShowAll();
			}

		public override Gtk.Widget Control {
			get { return exploreView; }
		}

		public override void Load(string fileName)
		{
		}

		public string PromptForLogin(string uri)
		{
			// first look in password cache
			string login = TeamFoundationSettings.Current.Get(uri);
			if (!String.IsNullOrEmpty(login)) return login;

			// now prompt
			AuthenticationDialog dialog = new AuthenticationDialog(uri);
			dialog.ShowAll();
			int rc = dialog.Run();

			login = dialog.Login;
			dialog.Destroy();

			if (rc != Convert.ToInt32(ResponseType.Ok)) return String.Empty;
			return login;
		}

		public ICredentials GetCredentials(Uri uri, ICredentials failedCredentials)
		{
			NetworkCredential creds = credentialCache.GetCredential(uri, "NTLM");
			if (creds != null) return creds;

			string login = PromptForLogin(uri.ToString());
			creds = new TFCredential(login);
			
			credentialCache.Add(uri, "NTLM", creds);
			return creds;
		}

		public void NotifyCredentialsAuthenticated (Uri uri)
		{
		}

		void MyShowChangesetEventHandler(object sender, ShowChangesetEventArgs args)
		{
			int changesetId = args.ChangesetId;
			VersionControlServer vcs = args.VersionControlServer;
			ChangesetVersionSpec versionSpec = new ChangesetVersionSpec(changesetId);

			string tname = System.IO.Path.GetTempFileName();
			using (StreamWriter sw = new StreamWriter(tname))
				{
					DiffOptions options = new DiffOptions();
					options.UseThirdPartyTool = false;
					options.Flags = DiffOptionFlags.EnablePreambleHandling;
					options.OutputType = DiffOutputType.Unified;
					options.TargetEncoding = Encoding.UTF8;
					options.SourceEncoding = Encoding.UTF8;
					options.StreamWriter = sw;
					options.StreamWriter.AutoFlush = true;

					DiffHelper.ShowChangeset(vcs, versionSpec, false, options);
				}

			Document d = MonoDevelop.Ide.Gui.IdeApp.Workbench.OpenDocument (tname, true);
			d.FileName = "Changeset " + changesetId.ToString();

			File.Delete(tname);
		}

		void MyShowFileEventHandler(object sender, ShowFileEventArgs args)
		{
			string serverItem = args.ServerItem;
			VersionControlServer vcs = args.VersionControlServer;

			Microsoft.TeamFoundation.VersionControl.Client.Item item = vcs.GetItem(serverItem, VersionSpec.Latest, 0, true);
			string tname = System.IO.Path.GetTempFileName();
			item.DownloadFile(tname);
				
			Document d = MonoDevelop.Ide.Gui.IdeApp.Workbench.OpenDocument (tname, true);
			d.FileName = serverItem;
			File.Delete(tname);
		}
	}
}
