using System;
using System.Security.Cryptography;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Mono.GetOptions;

[Command("difference", "Show pending changes, latest on server, a changeset, or local changes not pended as a unified diff.", 
				 "<path>...", "diff")]
class DifferenceCommand : Command
{
	[Option("Output only whether files differ", "q", "brief")]
		public bool OptionBrief = false;

	[Option("Look for modified files", "", "modified")]
		public bool OptionModified = false;

	[Option("Show out of date files (newer version on server)", "", "old")]
		public bool OptionOld = false;

	[Option("Ignore white space differences", "", "ignorespace")]
		public bool OptionIgnoreWhiteSpace = false;

	[Option("Owner name", "O", "owner")]
		public string OptionOwner;

	private MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

	public DifferenceCommand(Driver driver, string[] args): base(driver, args)
		{
		}

	protected DiffOptions GetDiffOptions()
	{
		DiffOptions options = new DiffOptions();
		options.UseThirdPartyTool = false;

		options.Flags = DiffOptionFlags.EnablePreambleHandling;
		if (OptionIgnoreWhiteSpace) options.Flags |= DiffOptionFlags.IgnoreWhiteSpace;

		options.OutputType = DiffOutputType.Unified;
		options.TargetEncoding = Console.OutputEncoding;
		options.SourceEncoding = Console.OutputEncoding;
		options.StreamWriter = new StreamWriter(Console.OpenStandardOutput(), 
																						Console.OutputEncoding);
		options.StreamWriter.AutoFlush = true;

		return options;
	}

	public void ShowModifiedFiles(Workspace workspace, string path)
	{
		char[] charsToTrim = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
		string itemPath = path.TrimEnd(charsToTrim);

		workspace.RefreshMappings();
		string serverPath = workspace.GetServerItemForLocalItem(itemPath);

		// pull item list based on WorkspaceVersion. otherwise might get
		// new items on server that haven't been pulled yet in the list returned
		WorkspaceVersionSpec version = new WorkspaceVersionSpec(workspace);

		// get item list from TFS server
		ItemSpec itemSpec = new ItemSpec(itemPath, RecursionType.Full);
		ItemSet itemSet = VersionControlServer.GetItems(itemSpec, version, DeletedState.NonDeleted, ItemType.Any, true);
		Item[] items = itemSet.Items;
		
		foreach (Item item in items)
			{
				if (item.ItemType != ItemType.File) continue;
				if (item.ServerItem.Length == serverPath.Length) continue;
				string serverItem = item.ServerItem.Remove(0, serverPath.Length+1);

				// server item paths are separated with '/', but on windows the file list below has '\' separated paths
				if (Path.DirectorySeparatorChar != '/')
					serverItem = serverItem.Replace('/', Path.DirectorySeparatorChar);

				// only looking for modifications, not deletes or adds
				string fname = Path.Combine(itemPath, serverItem);
				if (!File.Exists(fname)) continue;
				if (FileAttributes.ReadOnly == (File.GetAttributes(fname) & FileAttributes.ReadOnly))
					continue;

				using (FileStream fileStream = new FileStream(fname, FileMode.Open, FileAccess.Read))
					{
						string localHash = Convert.ToBase64String(md5.ComputeHash(fileStream));
						string itemHash = Convert.ToBase64String(item.HashValue);
						if (itemHash == localHash) continue;
					}

				string p = fname.Substring(path.Length+1);
				if (OptionBrief) 
					{
						Driver.WriteLine(CanonicalPath(p));
						continue;
					}

				string tnameA = Path.GetTempFileName();
				item.DownloadFile(tnameA);
				IDiffItem a = new DiffItemLocalFile(tnameA, item.Encoding, DateTime.Now, false);

				IDiffItem b = new DiffItemLocalFile(fname, item.Encoding, DateTime.Now, false);

				Difference.DiffFiles(VersionControlServer, a, b,
														 GetDiffOptions(), p, true);

				if (!String.IsNullOrEmpty(tnameA)) File.Delete(tnameA);
			}
	}

	public void ShowOldFiles(Workspace workspace, string path)
	{
		// process command options
		ItemSpec itemSpec = new ItemSpec(path, RecursionType.Full);

		List<ItemSpec> itemSpecs = new List<ItemSpec>();
		itemSpecs.Add(itemSpec);

		ExtendedItem[][] items = workspace.GetExtendedItems(itemSpecs.ToArray(),
																												DeletedState.NonDeleted, ItemType.Any);

		foreach (ExtendedItem[] itemArray in items)
			{
				foreach (ExtendedItem item in itemArray)
					{
						if (item.IsLatest) continue;

						string p = item.LocalItem.Substring(path.Length);
						if (OptionBrief) 
							{
								Driver.WriteLine(p);
								continue;
							}

						IDiffItem a = new DiffItemNull();
						IDiffItem b = new DiffItemNull();

						if ((item.ChangeType & ChangeType.Add) != ChangeType.Add)
							{
								a = new DiffItemLocalFile(item.LocalItem, item.Encoding,
																					DateTime.Now, false);
							}

						if ((item.ChangeType & ChangeType.Delete) != ChangeType.Delete)
							{
								b = new DiffItemVersionedFile(VersionControlServer,
																							item.ItemId, item.VersionLatest, item.LocalItem);
							}

						Difference.DiffFiles(VersionControlServer, a, b,
																 GetDiffOptions(), p, true);
					}
			}
	}

	void ShowPendingChanges(Workspace workspace, string path)
	{
		PendingChange[] pendingChanges = workspace.GetPendingChanges(path, RecursionType.Full, true);
		if (pendingChanges.Length == 0)
			{
				Console.WriteLine("No pending changes.");
				Environment.Exit((int)ExitCode.PartialSuccess);
			}

		string cwd = Environment.CurrentDirectory;
		foreach (PendingChange change in pendingChanges)
			{
				string p = change.LocalItem;
				if (p.StartsWith(cwd)) p = p.Substring(cwd.Length+1);

				if (OptionBrief) 
					{
						Driver.WriteLine(CanonicalPath(p));
						continue;
					}

				IDiffItem a = new DiffItemNull();
				IDiffItem b = new DiffItemNull();

				string tname = null;
				if (!change.IsAdd)
					{
						tname = Path.GetTempFileName();
						change.DownloadBaseFile(tname);

						a = new DiffItemLocalFile(tname, change.Encoding,
																			change.CreationDate, true);
					}

				if (!change.IsDelete)
					{
						b = new DiffItemLocalFile(change.LocalItem, change.Encoding,
																			change.CreationDate, false);
					}

				Difference.DiffFiles(VersionControlServer, a, b,
														 GetDiffOptions(), p, true);

				if (!String.IsNullOrEmpty(tname))
					File.Delete(tname);
			}
	}

	public override void Run()
	{
		string path = Environment.CurrentDirectory;
		if (Arguments.Length > 0)
			{
				path = Path.GetFullPath(Arguments[0]);
			}

		Workspace workspace = GetWorkspaceFromCache();

		if (OptionOld)
			{
				ShowOldFiles(workspace, path);
				Environment.Exit((int)ExitCode.Success);
			}

		if (OptionModified)
			{
				ShowModifiedFiles(workspace, path);
				Environment.Exit((int)ExitCode.Success);
			}

		if (File.Exists(path) || Directory.Exists(path))
			ShowPendingChanges(workspace, path);
		else
			{
				VersionSpec versionSpec = VersionSpec.ParseSingleSpec(Arguments[0], OwnerFromString(OptionOwner));
				if (versionSpec is ChangesetVersionSpec)
					DiffHelper.ShowChangeset(VersionControlServer,  
																	 versionSpec as ChangesetVersionSpec,
																	 OptionBrief, GetDiffOptions());
			}
	}
}
