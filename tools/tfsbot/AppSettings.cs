using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Text;
using System.Threading;

static class AppSettings
{
	static private string ircServer;
	static private string tfsUrl;
	static private string channel;
	static private string username;
	static private string password;
	static private string domain;
	static private string nick;
	static private int latestChangesetId = 0;

	static public string IrcServer
	{
		get { return ircServer; }
	}

	static public string Nick
	{
		get { return nick; }
	}

	static public string TfsUrl
	{
		get { return tfsUrl; }
	}

	static public string Channel
	{
		get { return channel; }
	}

	static public string Username
	{
		get { return username; }
	}

	static public string Password
	{
		get { return password; }
	}

	static public string Domain
	{
		get { return domain; }
	}

	static public int LatestChangesetId
	{
		get { return latestChangesetId; }
	}

	static AppSettings()
	{
		// Get the AppSettings collection.
		NameValueCollection appSettings =	ConfigurationManager.AppSettings;
		string[] keys = appSettings.AllKeys;

		// Loop to get key/value pairs.
		for (int i = 0; i < appSettings.Count; i++)
			{
	 				switch (keys[i])
					{
					case "irc.server": 
						ircServer = appSettings[i];
						break;
					case "irc.channel": 
						channel = appSettings[i];
						break;
					case "irc.nick": 
						nick = appSettings[i];
						break;
					case "tfs.url": 
						tfsUrl = appSettings[i];
						break;
					case "tfs.domain": 
						domain = appSettings[i];
						break;
					case "tfs.username": 
						username = appSettings[i];
						break;
					case "tfs.password": 
						password = appSettings[i];
						break;
					case "tfs.changeset": 
						string cid = appSettings[i];
						if (!String.IsNullOrEmpty(cid))
							latestChangesetId = Convert.ToInt32(cid);
						break;
					}
			}
	}
}
