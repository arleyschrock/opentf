using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using MonoDevelop.Core;
using MonoDevelop.VersionControl;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Server;

namespace MonoDevelop.VersionControl.TeamFoundation
{
	class TeamFoundationVersionControl : VersionControlSystem
	{
		private TeamFoundationServer _tfs;
		private VersionControlServer _versionControl;

		readonly string[] protocolsTfs = {"http"};
		
		public VersionControlServer VersionControlServer
		{
			get { return _versionControl; }
		}

		public override string Name {
			get { return "Team Foundation"; }
		}
		
		public override bool IsInstalled {
			get {	return true; }
		}
		
		protected override Repository OnCreateRepositoryInstance ()
		{
			Console.WriteLine("OnCreateRepositoryInstance");
			return new TeamFoundationRepository ();
		}
		
		public override Gtk.Widget CreateRepositoryEditor (Repository repo)
		{
			return new UrlBasedRepositoryEditor ((TeamFoundationRepository)repo, protocolsTfs);
		}
		
		public override Repository GetRepositoryReference (string path, string id)
		{
			WorkspaceInfo info = Workstation.Current.GetLocalWorkspaceInfo(path);
			Console.WriteLine("GetRepositoryReference : " + path);
			if (info == null) return new TeamFoundationRepository (this, "");

			Console.WriteLine("GetRepositoryReference : " + info.ServerUri.ToString());
			return new TeamFoundationRepository (this, info.ServerUri.ToString());
		}
		
		public override void StoreRepositoryReference (Repository repo, string path, string id)
		{
			Console.WriteLine("StoreRepositoryReference " + path + ", " + id);
			// Nothing to do
		}
		
		string GetTextBase(string sourcefile) {
			return Path.Combine(
				Path.Combine(
					Path.Combine(
						Path.GetDirectoryName(sourcefile),
						 ".svn"),
					"text-base"),
				Path.GetFileName(sourcefile) + ".svn-base"); 
		}
	
		internal static string GetDirectoryDotSvn (string sourcepath) {
			return Path.Combine(sourcepath, ".svn");
		}
		
		public bool IsDiffAvailable (Repository repo, string sourcefile) {
			try {
				return File.Exists(GetTextBase(sourcefile))
					&& IsVersioned(sourcefile)
					&& GetVersionInfo (repo, sourcefile, false).Status == VersionStatus.Modified;
			} catch {
				// GetVersionInfo may throw an exception
				return false;
			}
		}
		
		public bool IsVersioned (string sourcefile)
		{
			Console.WriteLine("IsVersioned: " + sourcefile);
			ItemSpec itemSpec = new ItemSpec(sourcefile, RecursionType.None);
			ItemSet itemSet = VersionControlServer.GetItems(itemSpec, VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Any, true);
			Item[] items = itemSet.Items;
			foreach (Item item in items)
				{
					Console.WriteLine(item.ToString());
				}

			return true;
		}

		public bool CanCommit (Repository repo, string sourcepath)
		{
			Console.WriteLine("CanCommit: " + sourcepath);
				if (Directory.Exists (sourcepath) && Directory.Exists (GetDirectoryDotSvn (sourcepath)))
					return true;
			if (GetVersionInfo (repo, sourcepath, false) != null)
				return true;
			return false;
		}
		
		public bool CanAdd (Repository repo, string sourcepath)
		{
			Console.WriteLine("CanAdd: " + sourcepath);
			// Do some trivial checks
			if (!Directory.Exists (GetDirectoryDotSvn (Path.GetDirectoryName (sourcepath))))
				return false;

			if (File.Exists (sourcepath)) {
				if (File.Exists (GetTextBase (sourcepath)))
					return false;
			} else if (Directory.Exists (sourcepath)) {
				if (Directory.Exists (GetTextBase (sourcepath)))
					return false;
			} else
				return false;
				
			// Allow adding only if the path is not already scheduled for adding
			
			VersionInfo ver = this.GetVersionInfo (repo, sourcepath, false);
			if (ver == null)
				return true;
			return ver.Status == VersionStatus.Unversioned;
		}
		
		public string GetPathToBaseText(string sourcefile) {
			Console.WriteLine("GetPathToBaseText: " + sourcefile);
			return GetTextBase(sourcefile);
		}
		
		//GetHistory http://10.100.1.88:8080/
		//MonoDevelop.VersionControl.TeamFoundation.TeamFoundationRepository
		//GetHistory for /home/jreed/Source/tfs-lsg-1.0/base/backend/web/Application.cs

		public Revision[] GetHistory (Repository repo, string sourcefile, Revision since) {
			ArrayList revs = new ArrayList();
			TeamFoundationRepository tfRepo = repo as TeamFoundationRepository;
			VersionControlServer vcs = tfRepo.VersionControlServer;

			IEnumerable changeSets = vcs.QueryHistory(sourcefile, VersionSpec.Latest, 0, RecursionType.None, null, 
																								null, null, 256, false, false, false);

			foreach (Changeset changeSet in changeSets)
				{
					revs.Add(new TfRevision(repo, changeSet));
				}

			return (Revision[])revs.ToArray(typeof(Revision));
		}
		
		public string GetTextAtRevision (string repositoryPath, Revision revision) 
		{
			return "NOT IMPLEMENTED!";
		}
		
		public VersionInfo GetVersionInfo (Repository repo, string localPath, bool getRemoteStatus)
		{
			if (File.Exists (GetTextBase(localPath)) || File.Exists (localPath))
				return GetFileStatus (repo, localPath, getRemoteStatus);
			else if (Directory.Exists (GetDirectoryDotSvn (localPath)) || Directory.Exists (localPath))
				return GetDirStatus (repo, localPath, getRemoteStatus);
			else
				return null;
		}
		
		private VersionInfo GetFileStatus (Repository repo, string sourcefile, bool getRemoteStatus)
		{
			// If the directory is not versioned, there is no version info
			if (!Directory.Exists (GetDirectoryDotSvn (Path.GetDirectoryName (sourcefile))))
				return null;
				
			IList statuses = null; //Client.Status(sourcefile, SvnClient.Rev.Head, false, false, getRemoteStatus);
			if (statuses.Count == 0)
				throw new ArgumentException("Path '" + sourcefile + "' does not exist in the repository.");
			
			return null;

// 			SvnClient.StatusEnt ent;
// 			if (statuses.Count != 1)
// 				throw new ArgumentException("Path '" + sourcefile + "' does not refer to a file in the repository.");
// 			ent = (SvnClient.StatusEnt)statuses[0];
// 			if (ent.IsDirectory)
// 				throw new ArgumentException("Path '" + sourcefile + "' does not refer to a file.");
// 			return CreateNode (ent, repo);
		}
		
		private VersionInfo GetDirStatus (Repository repo, string localPath, bool getRemoteStatus)
		{
			string parent = Path.GetDirectoryName (localPath);
			
			// If the directory is not versioned, there is no version info
			if (!Directory.Exists (GetDirectoryDotSvn (parent)))
				return null;
				
// 			IList statuses = null;//Client.Status (parent, SvnClient.Rev.Head, false, false, getRemoteStatus);
// 			foreach (SvnClient.StatusEnt ent in statuses) {
// 				if (ent.LocalFilePath == localPath)
// 					return CreateNode (ent, repo);
// 			}
			return null;
		}
		
		public VersionInfo[] GetDirectoryVersionInfo (Repository repo, string sourcepath, bool getRemoteStatus, bool recursive) {
			Console.WriteLine("in GetDirectoryVersionInfo");
			//IList ents = null; //Client.Status(sourcepath, SvnClient.Rev.Head, recursive, true, getRemoteStatus);
			return null;
			//			return CreateNodes (repo, ents);
		}
		
		public void Update(string path, bool recurse, IProgressMonitor monitor) {
			Console.WriteLine("TeamFoundationVersionControl.cs: Update");
		}
		
		public void Commit(string[] paths, string message, IProgressMonitor monitor) {
			Console.WriteLine("TeamFoundationVersionControl.cs: Commit");
		}
		
		public void Mkdir(string[] paths, string message, IProgressMonitor monitor) {
			Console.WriteLine("TeamFoundationVersionControl.cs: Mkdir");
		}
		
		public void Revert (string[] paths, bool recurse, IProgressMonitor monitor) {
			Console.WriteLine("TeamFoundationVersionControl.cs: Revert");
		}
		
		public void Add(string path, bool recurse, IProgressMonitor monitor) {
			Console.WriteLine("TeamFoundationVersionControl.cs: Add");
		}
		
		public void Delete(string path, bool force, IProgressMonitor monitor) {
			Console.WriteLine("TeamFoundationVersionControl.cs: Delete");
		}
		
		public void Move(string srcPath, string destPath, bool force, IProgressMonitor monitor) {
			Console.WriteLine("TeamFoundationVersionControl.cs: Move");
		}
		
		public string PathDiff (string path, bool recursive)
		{
			Console.WriteLine("TeamFoundationVersionControl.cs: PathDiff");
			return "NOT IMPLEMENTED";
		}
		
// 		private VersionInfo CreateNode (SvnClient.StatusEnt ent, Repository repo) 
// 		{
// 			VersionStatus rs = VersionStatus.Unversioned;
// 			Revision rr = null;
			
// 			VersionInfo ret = new VersionInfo(
// 				ent.LocalFilePath,
// 				ent.Url,
// 				ent.IsDirectory,
// 				ConvertStatus(ent.Schedule, ent.TextStatus),
// 				null, // TfRevision
// 				rs,
// 				rr
// 			);
			
// 			return ret;
// 		}
		
// 		private VersionInfo[] CreateNodes (Repository repo, IList ent) {
// 			VersionInfo[] ret = new VersionInfo[ent.Count];
// 			for (int i = 0; i < ent.Count; i++)
// 				ret[i] = CreateNode((SvnClient.StatusEnt)ent[i], repo);
// 			return ret;
// 		}
		
// 		private VersionStatus ConvertStatus(SvnClient.NodeSchedule schedule, SvnClient.VersionStatus status) {
// 			switch (schedule) {
// 				case SvnClient.NodeSchedule.Add: return VersionStatus.ScheduledAdd;
// 				case SvnClient.NodeSchedule.Delete: return VersionStatus.ScheduledDelete;
// 				case SvnClient.NodeSchedule.Replace: return VersionStatus.ScheduledReplace;
// 			}

// 			switch (status) {
// 				case SvnClient.VersionStatus.None: return VersionStatus.Unchanged;
// 				case SvnClient.VersionStatus.Normal: return VersionStatus.Unchanged;
// 				case SvnClient.VersionStatus.Unversioned: return VersionStatus.Unversioned;
// 				case SvnClient.VersionStatus.Modified: return VersionStatus.Modified;
// 				case SvnClient.VersionStatus.Merged: return VersionStatus.Modified;
// 				case SvnClient.VersionStatus.Conflicted: return VersionStatus.Conflicted;
// 				case SvnClient.VersionStatus.Ignored: return VersionStatus.UnversionedIgnored;
// 				case SvnClient.VersionStatus.Obstructed: return VersionStatus.Obstructed;
// 			}
// 			return VersionStatus.Unversioned;
// 		}
	}
	

	public class TeamFoundationException : ApplicationException {
		public TeamFoundationException(string message) : base(message) { }
	}
}
