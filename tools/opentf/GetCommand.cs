//
// GetCommand.cs
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Mono.GetOptions;
using OpenTF.Common;

[Command("get", "Update local repository copy with latest versions from the server.", "<path>...")]
class GetCommand : Command
{
	[Option("Get all files, not just those out of date", "", "all")]
		public bool OptionAll = false;

	[Option("Force operation", "P", "force")]
		public bool OptionForce = false;

	[Option("Recursive", "R", "recursive")]
		public bool OptionRecursive = false;

	[Option("Version", "V", "version")]
		public string OptionVersion;

	[Option("Overwrite files that are writable", "", "overwrite")]
		public bool OptionOverwrite = false;

	SortedList<string, int> fileList = new SortedList<string, int>();
	Workspace workspace;

	public GetCommand(Driver driver, string[] args): base(driver, args)
		{
			workspace = GetWorkspaceFromCache();
			VersionControlServer.Getting += MyGettingEventHandler;
		}

	public void MyGettingEventHandler(Object sender, GettingEventArgs e)
	{
		if (e.DeletionId != 0)
			{
				Console.WriteLine("deleting " + CanonicalPath(e.SourceLocalItem));
				return;
			}
		
		if ((!String.IsNullOrEmpty(e.TargetLocalItem))&&
				(!String.IsNullOrEmpty(e.SourceLocalItem))&&
				(e.SourceLocalItem != e.TargetLocalItem))
			Console.WriteLine("renaming " + CanonicalPath(e.TargetLocalItem));
		else
			Console.WriteLine("updating " + CanonicalPath(e.TargetLocalItem));

		if (e.ItemType == ItemType.Folder) return;
		fileList.Add(e.TargetLocalItem, e.Version);
	}	

	public GetOptions GetOptionFlags()
	{
		GetOptions getOptions = GetOptions.None;
		if (OptionAll || OptionForce) getOptions |= GetOptions.GetAll;
		if (OptionOverwrite || OptionForce) getOptions |= GetOptions.Overwrite;
		return getOptions;
	}

	public GetStatus UpdatePathFromServer(string[] paths)
	{
		// process command options
		RecursionType folderRecursion = OptionRecursive ? RecursionType.Full : RecursionType.OneLevel;
		bool getSetting = Settings.Current.GetAsBool("Get.Recursive");
		if (getSetting) folderRecursion = RecursionType.Full;

		VersionSpec version = VersionFromString(OptionVersion);
		List<GetRequest> requests = new List<GetRequest>();

		foreach (string path in paths)
			{
				RecursionType recursion = RecursionType.None;
				if (Directory.Exists(path)) recursion = folderRecursion;

				if (path[0] != '$')
					requests.Add(new GetRequest(Path.GetFullPath(path), recursion, version));
				else
					requests.Add(new GetRequest(path, recursion, version));
			}

		return workspace.Get(requests.ToArray(), GetOptionFlags());
	}

	public void SetPermissions(SortedList<string, int> fileList)
	{
		Console.Write("Setting permissions ...");

		int i = 0;
		foreach (string file in fileList.Keys)
			{
				if (0 == (i % 100)) Console.Write("."); 
				i++;

				if (! FileTypeDatabase.ShouldBeExecutable(file)) continue;
				FileType.MakeExecutable(file);
			}

		Console.WriteLine();
		Console.WriteLine("Done!");
	}

	public override void Run()
	{
		List<string> paths = new List<string>();
		foreach (string p in Arguments) paths.Add(p);

		if (paths.Count == 0 && Settings.Current.GetAsBool("Get.DefaultToCwd"))
			paths.Add(Environment.CurrentDirectory);

		GetStatus status;
		if (paths.Count > 0)
			status = UpdatePathFromServer(paths.ToArray());
		else 
			status = workspace.Get(VersionFromString(OptionVersion), GetOptionFlags());

		if (status.NumOperations == 0)
			{
				Console.WriteLine("Nothing to do.");
				Environment.Exit((int)ExitCode.PartialSuccess);
			}

		SetPermissions(fileList);
	}
}
