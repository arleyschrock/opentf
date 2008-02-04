using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

using Meebey.SmartIrc4net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using OpenTF.Common;

class NotifierDaemon
{
	private IrcClient irc = new	IrcClient();
	private int port = 6667;

	private static void Main(string[] arguments)
	{
		NotifierDaemon nd = new NotifierDaemon();
		nd.Run();
	}

	private void UpdateLatestChangset(int cid)
	{
		// Open App.Config of executable
		System.Configuration.Configuration config =
			ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

		KeyValueConfigurationCollection settings = config.AppSettings.Settings;
		settings.Remove("tfs.changeset");
		settings.Add("tfs.changeset", cid.ToString());

		// Save the configuration file.
		config.Save(ConfigurationSaveMode.Modified);
	}

	public void Run()
	{
		if (AppSettings.LatestChangesetId == 0)
			UpdateLatestChangset(VersionControlServer.GetLatestChangesetId() - 1);
	
		// Force a reload of a changed section.
		ConfigurationManager.RefreshSection("appSettings");
		irc.OnConnected += new EventHandler(OnConnected);
		irc.OnChannelMessage += new IrcEventHandler(OnChannelMessage);

		try
			{
				irc.Connect(AppSettings.IrcServer, port);
				irc.SendDelay = 1000;
				new Thread(new ThreadStart(MonitorCheckins)).Start();
				irc.Listen();
			}
		catch (Exception e)
			{
				Console.Write("Failed to connect: "+ e.Message);
			}
	}

	private VersionControlServer VersionControlServer
	{
		get 
			{
				NetworkCredential creds = new NetworkCredential(AppSettings.Username, AppSettings.Password, AppSettings.Domain);
				TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer(AppSettings.TfsUrl, creds);
				return tfs.GetService(typeof(VersionControlServer)) as VersionControlServer;
			}
	}

	void OnConnected(object sender, EventArgs e)
	{
		irc.Login(AppSettings.Nick, "Team Foundation Notification Daemon");
		irc.RfcJoin(AppSettings.Channel);
	}

	void OnChannelMessage(object sender, IrcEventArgs e)
	{
		if ((e.Data.MessageArray.Length == 0) || (e.Data.MessageArray[0] != "tfs")) return;

		if (e.Data.MessageArray.Length < 2)
			{
				irc.SendMessage(SendType.Message, e.Data.Nick, "No commands implemented yet!");
			}
	}

	void MonitorCheckins()
	{
		int lastestChangesetId = AppSettings.LatestChangesetId;

		while (true)
			{
				ChangesetVersionSpec versionFrom = new ChangesetVersionSpec(lastestChangesetId.ToString());
				IEnumerable changeSets = VersionControlServer.QueryHistory(VersionControlPath.RootFolder, 
																																	 VersionSpec.Latest, 0, RecursionType.Full, 
																																	 null, versionFrom, null, 100, false, false, false);
		
				Stack<string> msgStack = new Stack<string>();

				foreach (Changeset changeSet in changeSets)
					{
						if (changeSet.ChangesetId > lastestChangesetId) 
							{
								lastestChangesetId = changeSet.ChangesetId;
						
								//StringBuilder sb = new StringBuilder();
								//foreach (Change change in changeSet.Changes)
								//	{
								//		if (sb.Length != 0) sb.Append(", ");
								//		sb.Append(change.Item.ServerItem);
								//	}
								// msgStack.Push("Files: " + sb.ToString());

								string msg0 = String.Format("Url: {0}/VersionControl/Changeset.aspx?artifactMoniker={1}&webView=true",
								AppSettings.TfsUrl, changeSet.ChangesetId);
								msgStack.Push(msg0);

								if (!String.IsNullOrEmpty(changeSet.Comment))
									{
										string msg1 = String.Format("Comment: {0}", changeSet.Comment);
										msgStack.Push(msg1);
									}

								string msg2 = String.Format("Changset: {0} by {1} on {2}", 
																						changeSet.ChangesetId,
																						changeSet.Committer, 
																						changeSet.CreationDate.ToString());
								msgStack.Push(msg2);
							}
					}

				bool fnd = msgStack.Count > 0;
				while (msgStack.Count > 0)
					{
						string msg = msgStack.Pop();
						irc.SendMessage(SendType.Message, AppSettings.Channel, msg);
					}

				if (fnd) UpdateLatestChangset(lastestChangesetId);
				Thread.Sleep(1000*300);
			}
	}
}