//
// LockCommand.cs
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

[Command("lock", "Lock file(s) in the repository.", "<path>...")]
class LockCommand : Command
{
	[Option("Lock Type (None, Checkin, or Checkout)", "", "lock")]
		public string OptionLock;

	[Option("Recursive", "R", "recursive")]
		public bool OptionRecursive = false;

	public LockCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	private void InvalidLockLevel()
	{
		Console.WriteLine("Specify the lock with the /lock:<type> option.");
		Console.WriteLine("Valid lock types are: None, Checkin, or Checkout.");
		Console.WriteLine("Use /lock:None to remove a lock.");
		Environment.Exit((int)ExitCode.Failure);
	}

	LockLevel LockLevelFromString(string optionLock)
	{
		if (String.IsNullOrEmpty(optionLock)) InvalidLockLevel();

		if (optionLock.Equals("none", StringComparison.InvariantCultureIgnoreCase))
			return LockLevel.None;

		if (optionLock.Equals("checkin", StringComparison.InvariantCultureIgnoreCase))
			return LockLevel.Checkin;

		if (optionLock.Equals("checkout", StringComparison.InvariantCultureIgnoreCase))
			return LockLevel.CheckOut;

		InvalidLockLevel();
		return LockLevel.Unchanged;
	}

	public override void Run()
	{
		Workspace workspace = GetWorkspaceFromCache();
		List<string> paths;

		ConfirmFilesSpecified();
		paths = VerifiedFullPaths(Arguments);

		LockLevel lockLevel = LockLevelFromString(OptionLock);
		int rc = workspace.SetLock(paths.ToArray(), lockLevel, OptionRecursive ? RecursionType.Full : RecursionType.None);

		if (LockLevel.None == lockLevel)
			Console.WriteLine("{0} file(s) unlocked.", rc);
		else
			Console.WriteLine("{0} file(s) locked for {1}.", rc, lockLevel.ToString().ToLower());
	}
}
