using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
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
	class TeamFoundationRepository: UrlBasedRepository
	{
		private TeamFoundationServer _tfs;
		private VersionControlServer _versionControl;

		static Dictionary<string, string> repoConfig = new Dictionary<string, string>();

		static TeamFoundationRepository()
			{
				string configFile = Path.Combine (Runtime.Properties.ConfigDirectory, "VersionControl.config");
				XmlTextReader reader = new XmlTextReader (new StreamReader (configFile)); 
				while (reader.Read())
					{
						string serverUri;
						if (reader.Name == "Repository")
							{
								if (reader.GetAttribute("VcsType") != "MonoDevelop.VersionControl.TeamFoundation.TeamFoundationVersionControl")
									continue;
								
								serverUri = String.Format("{0}://{1}:{2}/", 
																					reader.GetAttribute("Method"),
																					reader.GetAttribute("Server"), 
																					reader.GetAttribute("Port"));

								//Console.WriteLine("Adding " + serverUri + " to repoConfig");
								repoConfig.Add(serverUri, reader.GetAttribute("User"));
							}
					}
			}

		public TeamFoundationRepository ()
			{
				Console.WriteLine("TeamFoundationRepository()");
				Method = "http";
			}
		
		public TeamFoundationRepository (TeamFoundationVersionControl vcs, string url): base (vcs)
			{
				Console.WriteLine("TeamFoundationRepository(vcs, " + Url + ")");
				Url = url;
			}
		
		public VersionControlServer VersionControlServer {
			get {

				if (_versionControl != null) return _versionControl;

				Uri repoUri = new Uri(Url);
				string serverUrl = String.Format("http://{0}:8080/", repoUri.Host);

				string userInfo = repoUri.UserInfo;
				if (String.IsNullOrEmpty(userInfo))
					{
						userInfo = repoConfig[serverUrl];
					}

				string username = "";
				string pwd = "";
				string domain = "";

				int colon = userInfo.IndexOf(":");
				if (colon != -1)
					{
						pwd = userInfo.Substring(colon+1);
						userInfo = userInfo.Substring(0, colon);
					}

				int slash = userInfo.IndexOf('\\');
				if (-1 == slash) username = userInfo;
				else
					{
						domain = userInfo.Substring(0, slash);
						username = userInfo.Substring(slash+1);
					}

				_tfs = TeamFoundationServerFactory.GetServer(serverUrl, new NetworkCredential(username, pwd, domain));
				_versionControl = (VersionControlServer) _tfs.GetService(typeof(VersionControlServer));
				return _versionControl;
			}
		}

		public override bool HasChildRepositories {
			get {
				Console.WriteLine("User: " + User);
				Console.WriteLine("Port: " + Port);
				Console.WriteLine("Pass: " + Pass);
				return true; 
			}
		}
		
		public override IEnumerable<Repository> ChildRepositories {
			get {
				Console.WriteLine("ChildRepositories for " + Url);
				Uri repoUri = new Uri(Url);
				string workspaceName = repoUri.PathAndQuery.Substring(1);
				
				string workspaceOwner;
				int colon = repoUri.UserInfo.IndexOf(":");
				if (colon != -1) workspaceOwner = repoUri.UserInfo.Substring(0, colon);
				else workspaceOwner = repoUri.UserInfo;

				Console.WriteLine("Workspace is " + workspaceName);
				Console.WriteLine("Owner is " + workspaceOwner);

				List<Repository> list = new List<Repository>();
				int dollar = Url.LastIndexOf("$/");
				if (dollar == -1)
					{
						Workspace workspace = VersionControlServer.GetWorkspace(workspaceName, workspaceOwner);
						foreach (WorkingFolder folder in workspace.Folders)
							{
								TeamFoundationRepository rep = new TeamFoundationRepository (Tfs, Url + "/" + folder.LocalItem);
								rep.Name = folder.LocalItem;
								list.Add (rep);
							}
					}
				else
					{
						// http://domain\uid:pwd@10.100.1.88:8080/neo/$/LSG-1.0/releases/4.2
						StringBuilder path = new StringBuilder("$");
						{
							path.Append(Url.Substring(dollar+1));
						}
						path.Append("/*");
						//Console.WriteLine("ChildRepositories: " + path);

						ItemSpec itemSpec = new ItemSpec(path.ToString(), RecursionType.None);
						ItemSet itemSet = VersionControlServer.GetItems(itemSpec, VersionSpec.Latest,
																														DeletedState.NonDeleted, 
																														ItemType.Folder, false);

						Item[] items = itemSet.Items;
						foreach (Item item in items)
							{
								int slash = item.ServerItem.LastIndexOf("/");
								string folder = item.ServerItem.Substring(slash+1);
								TeamFoundationRepository rep = new TeamFoundationRepository (Tfs, Url + "/" + folder);
								rep.Name = folder;
								list.Add (rep);
							}
					}

				return list;
			}
		}

		TeamFoundationVersionControl Tfs {
			get { return (TeamFoundationVersionControl) VersionControlSystem; }
		}
		
		public override bool IsModified (string sourcefile)
		{
			Console.WriteLine("TeamFoundationRepository.cs: IsModified");
			ItemSpec itemSpec = new ItemSpec(sourcefile, RecursionType.None);
			ItemSet itemSet = VersionControlServer.GetItems(itemSpec, VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Any, true);
			Item[] items = itemSet.Items;
			foreach (Item item in items)
				{
					Console.WriteLine(item.ToString());
				}
			return true;
			//			return Svn.IsDiffAvailable (this, sourcefile);
		}
		
		public override bool IsVersioned (string sourcefile)
		{
			return (null != Workstation.Current.GetLocalWorkspaceInfo(sourcefile));
		}
		
		public override bool CanAdd (string sourcepath)
		{
			Console.WriteLine("TeamFoundationRepository.cs: CanAdd");

			ItemSpec itemSpec = new ItemSpec(sourcepath, RecursionType.None);
			ItemSet itemSet = VersionControlServer.GetItems(itemSpec, VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Any, true);
			Item[] items = itemSet.Items;
			foreach (Item item in items)
				{
					Console.WriteLine(item.ToString());
				}

			if (IsVersioned (sourcepath) && File.Exists (sourcepath) && !Directory.Exists (sourcepath)) {
				VersionInfo srcInfo = GetVersionInfo (sourcepath, false);
				return srcInfo.Status == VersionStatus.ScheduledDelete;
			}

			return true;
			//return Svn.CanAdd (this, sourcepath);
		}
		
		public override bool CanCommit (string localPath)
		{
			Console.WriteLine("TeamFoundationRepository.cs: CanCommit " + localPath + " ?");

			return IsVersioned(localPath);
			//			return Svn.CanCommit (this, localPath);
		}
		
		public override string GetPathToBaseText (string sourcefile)
		{
			Console.WriteLine("TeamFoundationRepository.cs: GetPathToBaseText");
			return "";
			//			return Svn.GetPathToBaseText (sourcefile);
		}
		
		public override string GetTextAtRevision (string repositoryPath, Revision revision)
		{
			Console.WriteLine("TeamFoundationRepository.cs: GetTextAtRevision");
			return "";
			//return Svn.GetTextAtRevision (repositoryPath, revision);
		}
		
		public override Revision[] GetHistory (string sourcefile, Revision since)
		{
			Console.WriteLine("TeamFoundationRepository.cs: GetHistory");
			return null;
			//return Svn.GetHistory (this, sourcefile, since);
		}
		
		public override VersionInfo GetVersionInfo (string localPath, bool getRemoteStatus)
		{
			Console.WriteLine("TeamFoundationRepository.cs: GetVersionInfo");
			return null;
			//return Svn.GetVersionInfo (this, localPath, getRemoteStatus);
		}
		
		public override VersionInfo[] GetDirectoryVersionInfo (string sourcepath, bool getRemoteStatus, bool recursive)
		{
			Console.WriteLine("TeamFoundationRepository.cs: GetDirectoryVersionInfo");
			return null;
			//			return Svn.GetDirectoryVersionInfo (this, sourcepath, getRemoteStatus, recursive);
		}

		public override Repository Publish (string serverPath, string localPath, string[] files, string message, IProgressMonitor monitor)
		{
			Console.WriteLine("TeamFoundationRepository.cs: Publish");
			string url = Url;
			if (!serverPath.StartsWith ("/"))
				url += "/";
			url += serverPath;
			
			string[] paths = new string[] {url};
			
			CreateDirectory (paths, message, monitor);
			//			Svn.Checkout (this.Url + "/" + serverPath, localPath, null, true, monitor);

			Hashtable dirs = new Hashtable ();
			PublishDir (dirs, localPath, false, monitor);

			foreach (string file in files) {
				PublishDir (dirs, Path.GetDirectoryName (file), true, monitor);
				Add (file, false, monitor);
			}
			
			ChangeSet cset = CreateChangeSet (localPath);
			cset.AddFile (localPath);
			cset.GlobalComment = message;
			Commit (cset, monitor);
			
			return new TeamFoundationRepository (Tfs, paths[0]);
		}

		void PublishDir (Hashtable dirs, string dir, bool rec, IProgressMonitor monitor)
		{
			while (dir [dir.Length - 1] == Path.DirectorySeparatorChar)
				dir = dir.Substring (0, dir.Length - 1);

			if (dirs.ContainsKey (dir))
				return;

			dirs [dir] = dir;
			if (rec) {
				PublishDir (dirs, Path.GetDirectoryName (dir), true, monitor);
				Add (dir, false, monitor);
			}
		}

		public override void Update (string path, bool recurse, IProgressMonitor monitor)
		{
			Console.WriteLine("TeamFoundationRepository.cs: Update");
			Command cmd = new CommandUpdate(VersionControlServer, path, recurse, monitor);
			cmd.Run();
		}
		
		public override void Commit (ChangeSet changeSet, IProgressMonitor monitor)
		{
			Console.WriteLine("TeamFoundationRepository.cs: Commit");

			ArrayList list = new ArrayList ();
			foreach (ChangeSetItem it in changeSet.Items)
				list.Add (it.LocalPath);
			//Svn.Commit ((string[])list.ToArray (typeof(string)), changeSet.GlobalComment, monitor);
		}
		
		void CreateDirectory (string[] paths, string message, IProgressMonitor monitor)
		{
			Console.WriteLine("TeamFoundationRepository.cs: CreateDirectory");
			//Svn.Mkdir (paths, message, monitor);
		}
		
		public override void Checkout (string path, Revision rev, bool recurse, IProgressMonitor monitor)
		{
			Console.WriteLine("TeamFoundationRepository.cs: Checkout " + path + ", " + Url);

			Command cmd = new CommandCheckout(VersionControlServer, Url,
																				path, rev, recurse, monitor);
			cmd.Run();
		}

		public override void Revert (string localPath, bool recurse, IProgressMonitor monitor)
		{
			Console.WriteLine("TeamFoundationRepository.cs: Revert");
			//Svn.Revert (new string[] {localPath}, recurse, monitor);
		}
		
		public override void Add (string path, bool recurse, IProgressMonitor monitor)
		{
			if (IsVersioned (path) && File.Exists (path) && !Directory.Exists (path)) {
				VersionInfo srcInfo = GetVersionInfo (path, false);
				if (srcInfo.Status == VersionStatus.ScheduledDelete) {
					// It is a file that was deleted. It can be restored now since it's going
					// to be added again.
					// First of all, make a copy of the file
					string tmp = Path.GetTempFileName ();
					File.Copy (path, tmp, true);
					
					// Now revert the status of the file
					Revert (path, false, monitor);
					
					// Copy the file over the old one and clean up
					File.Copy (tmp, path, true);
					File.Delete (tmp);
				}
			}
			///else
				//Svn.Add (path, recurse, monitor);
		}
		
		public override void MoveFile (string srcPath, string destPath, bool force, IProgressMonitor monitor)
		{
			bool destIsVersioned = false;
			
			if (File.Exists (destPath))
				throw new InvalidOperationException ("Cannot move file. Destination file already exist.");

			if (IsVersioned (destPath)) {
				// Revert to the original status
				Revert (destPath, false, monitor);
				if (File.Exists (destPath))
					File.Delete (destPath);
				destIsVersioned = true;
			}
			
			VersionInfo srcInfo = GetVersionInfo (srcPath, false);
			if (srcInfo != null && srcInfo.Status == VersionStatus.ScheduledAdd) {
				// If the file is scheduled to add, cancel it, move the file, and schedule to add again
				Revert (srcPath, false, monitor);
				if (!destIsVersioned)
					MakeDirVersioned (Path.GetDirectoryName (destPath), monitor);
				base.MoveFile (srcPath, destPath, force, monitor);
				if (!destIsVersioned)
					Add (destPath, false, monitor);
			} else {
				if (!destIsVersioned && IsVersioned (srcPath)) {
					MakeDirVersioned (Path.GetDirectoryName (destPath), monitor);
					//Svn.Move (srcPath, destPath, force, monitor);
				} else
					base.MoveFile (srcPath, destPath, force, monitor);
			}
		}
		
		public override void MoveDirectory (string srcPath, string destPath, bool force, IProgressMonitor monitor)
		{
			if (IsVersioned (destPath))
				{
					VersionInfo vinfo = GetVersionInfo (destPath, false);
					if (vinfo.Status != VersionStatus.ScheduledDelete && Directory.Exists (destPath))
						throw new InvalidOperationException ("Cannot move directory. Destination directory already exist.");
					
					srcPath = Path.GetFullPath (srcPath);
				
					// The target directory does not exist, but it is versioned. It may be because
					// it is scheduled to delete, or maybe it has been physicaly deleted. In any
					// case we are going to replace the old directory by the new directory.
				
					// Revert the old directory, so we can see which files were there so
					// we can delete or replace them
					Revert (destPath, true, monitor);
				
					// Get the list of files in the directory to be replaced
					ArrayList oldFiles = new ArrayList ();
					GetDirectoryFiles (destPath, oldFiles);
				
					// Get the list of files to move
					ArrayList newFiles = new ArrayList ();
					GetDirectoryFiles (srcPath, newFiles);
				
					// Move all new files to the new destination
					Hashtable copiedFiles = new Hashtable ();
					Hashtable copiedFolders = new Hashtable ();
					foreach (string file in newFiles) {
						string src = Path.GetFullPath (file);
						string dst = Path.Combine (destPath, src.Substring (srcPath.Length + 1));
						if (File.Exists (dst))
							File.Delete (dst);
					
						// Make sure the target directory exists
						string destDir = Path.GetDirectoryName (dst);
						if (!Directory.Exists (destDir))
							Directory.CreateDirectory (destDir);
						
						// If the source file is versioned, make sure the target directory
						// is also versioned.
						if (IsVersioned (src))
							MakeDirVersioned (destDir, monitor);

						MoveFile (src, dst, true, monitor);
						copiedFiles [dst] = dst;
						string df = Path.GetDirectoryName (dst);
						copiedFolders [df] = df;
					}
				
					// Delete all old files which have not been replaced
					ArrayList foldersToDelete = new ArrayList ();
					foreach (string oldFile in oldFiles) {
						if (!copiedFiles.Contains (oldFile)) {
							DeleteFile (oldFile, true, monitor);
							string fd = Path.GetDirectoryName (oldFile);
							if (!copiedFolders.Contains (fd) && !foldersToDelete.Contains (fd))
								foldersToDelete.Add (fd);
						}
					}
				
					// Delete old folders
					//foreach (string folder in foldersToDelete) {
						//Svn.Delete (folder, true, monitor);
					//}
				
					// Delete the source directory
					DeleteDirectory (srcPath, true, monitor);
				}
			else {
				if (Directory.Exists (destPath))
					throw new InvalidOperationException ("Cannot move directory. Destination directory already exist.");
				
				VersionInfo srcInfo = GetVersionInfo (srcPath, false);
				if (srcInfo != null && srcInfo.Status == VersionStatus.ScheduledAdd) {
					// If the directory is scheduled to add, cancel it, move the directory, and schedule to add it again
					MakeDirVersioned (Path.GetDirectoryName (destPath), monitor);
					Revert (srcPath, true, monitor);
					base.MoveDirectory (srcPath, destPath, force, monitor);
					Add (destPath, true, monitor);
				} else {
					if (IsVersioned (srcPath)) {
						MakeDirVersioned (Path.GetDirectoryName (destPath), monitor);
						//Svn.Move (srcPath, destPath, force, monitor);
					} else
						base.MoveDirectory (srcPath, destPath, force, monitor);
				}
			}
		}
		
		void MakeDirVersioned (string dir, IProgressMonitor monitor)
		{
			if (Directory.Exists (Path.Combine (dir, ".svn")))
				return;
			
			// Make the parent versioned
			string parentDir = Path.GetDirectoryName (dir);
			if (parentDir == dir || parentDir == "")
				throw new InvalidOperationException ("Could not create versioned directory.");
			MakeDirVersioned (parentDir, monitor);
			
			Add (dir, false, monitor);
		}

		void GetDirectoryFiles (string directory, ArrayList collection)
		{
			string[] dir = Directory.GetDirectories(directory);
			foreach (string d in dir) {
				// Get all files ignoring the .svn directory
				if (Path.GetFileName (d) != ".svn")
					GetDirectoryFiles (d, collection);
			}
			string[] file = Directory.GetFiles (directory);
			foreach (string f in file)
				collection.Add(f);
		}

		
		public override void DeleteFile (string path, bool force, IProgressMonitor monitor)
		{
			//if (IsVersioned (path))
				//Svn.Delete (path, force, monitor);
			//else
				base.DeleteFile (path, force, monitor);
		}
		
		public override void DeleteDirectory (string path, bool force, IProgressMonitor monitor)
		{
			//if (IsVersioned (path))
				//Svn.Delete (path, force, monitor);
			//else
				base.DeleteDirectory (path, force, monitor);
		}
		
		public override DiffInfo[] PathDiff (string baseLocalPath, string[] localPaths)
		{
			if (localPaths != null) {
				ArrayList list = new ArrayList ();
				foreach (string path in localPaths) {
					string diff = "";//Svn.PathDiff (path, false);
					if (diff == null)
						continue;
					try {
						list.AddRange (GenerateUnifiedDiffInfo (diff, path, new string [] { path }));
					} finally {
						Runtime.FileService.DeleteFile (diff);
					}
				}
				return (DiffInfo[]) list.ToArray (typeof(DiffInfo));
			} else {
				string diff = ""; //Svn.PathDiff (baseLocalPath, true);
				try {
					return GenerateUnifiedDiffInfo (diff, baseLocalPath, null);
				} finally {
					Runtime.FileService.DeleteFile (diff);
				}
			}
		}
	}
}
