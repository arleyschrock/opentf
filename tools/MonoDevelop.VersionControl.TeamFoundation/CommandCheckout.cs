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
	class CommandCheckout : Command
	{
		private string _serverPath;
		private RecursionType recursionType;
		private GetRequest getRequest;

		public CommandCheckout (VersionControlServer vcs, string serverPath,
														string localPath, Revision rev,
														bool recurse, IProgressMonitor monitor) : base(vcs)
		{
			if (String.IsNullOrEmpty(serverPath))
				{
					throw new ArgumentException("No server path specified");
				}

			_serverPath = serverPath;
			recursionType = recurse ? RecursionType.Full : RecursionType.None;
			getRequest = new GetRequest(serverPath, recursionType, VersionSpec.Latest);
		}

		public void MyGettingEventHandler(Object sender, GettingEventArgs e)
		{
			if (e.Status == OperationStatus.Getting)
				Console.WriteLine("updating {0}", e.TargetLocalItem);
		}	

		void ProcessGetOutput(Workspace workspace, ILocalUpdateOperation[] operations,
													object userData)
		{
			List<string> setPermissions = (List<string>) userData;
			foreach (ILocalUpdateOperation operation in operations)
				{
					if (operation.ItemType == ItemType.Folder) continue;

					switch (operation.ChangeType)
						{
						case ChangeType.Delete:
							Console.WriteLine("deleting " + operation.SourceLocalItem);
							break;
						case ChangeType.Rename:
							Console.WriteLine("renaming " + operation.TargetLocalItem);
							setPermissions.Add(operation.TargetLocalItem);
							break;
						case ChangeType.Add:
							Console.WriteLine("adding " + operation.TargetLocalItem);
							setPermissions.Add(operation.TargetLocalItem);
							break;
						default:
							break;
						}
				}
		}

		public override void Run ()
		{
			VersionControlServer.Getting += MyGettingEventHandler;
			//List<string> setPermissions = new List<string>();
			//GetOptions getOptions = GetOptions.None;

			//Workspace workspace = GetWorkspace();
			//workspace.Get(getRequest, getOptions, ProcessGetOutput, setPermissions);
		}
	}
}
