using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Collections;
using MonoDevelop.Projects.Serialization;
using MonoDevelop.Core;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Server;

namespace MonoDevelop.VersionControl.TeamFoundation
{
	abstract class Command
	{
		private VersionControlServer _vcs;

		public Command(VersionControlServer vcs)
		{
			_vcs = vcs;
		}

		public VersionControlServer VersionControlServer
		{
			get { return _vcs; }
		}

		public abstract void Run ();
	}
}
