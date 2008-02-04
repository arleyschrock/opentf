using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Gtk;

using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Components.Commands;
using MonoDevelop.VersionControl.Views;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Gtk.TeamFoundation;

namespace MonoDevelop.TeamFoundation
{
	public enum TeamFoundationCommands
	{
		ShowTeamFoundation
	}
	
	public class ShowTeamFoundationHandler : CommandHandler
	{
		protected override void Run()
		{
			foreach (Document d in IdeApp.Workbench.Documents) {
				if (d.GetContent<TeamFoundationView>() != null) {
					d.Select();
					return;
				}
			}

			TeamFoundationView tfv = new TeamFoundationView();
			IdeApp.Workbench.OpenDocument(tfv, true);
		}
	}
}