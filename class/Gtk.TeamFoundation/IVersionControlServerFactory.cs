using System;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Gtk.TeamFoundation
{
	public interface IVersionControlServerFactory
	{
		VersionControlServer GetVersionControlServer(string url);
	}
}
