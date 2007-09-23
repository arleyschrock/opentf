//
//UnlabelCommand.cs
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

[Command("unlabel", "Remove a label or tag from an item.", "<label> <itemspec>")]
class UnlabelCommand : Command
{
	[Option("Recursive", "R", "recursive")]
		public bool OptionRecursive = false;

	[Option("Version", "V", "version")]
		public string OptionVersion;

	public UnlabelCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public override void Run()
	{
		if (Arguments.Length < 2)
			{
				Console.WriteLine("Usage: tf unlabel <label> <itemSpec>");
				Environment.Exit(0);
			}

		string labelName = Arguments[0];
		string itemPath = Path.GetFullPath(Arguments[1]);
		
		// parse arguments
		RecursionType rtype = OptionRecursive ? RecursionType.Full : RecursionType.None;

		List<ItemSpec> itemSpecs = new List<ItemSpec>();
		itemSpecs.Add(new ItemSpec(itemPath, rtype));

		LabelResult[] results = VersionControlServer.UnlabelItem(labelName, null,
																														 itemSpecs.ToArray(), 
																														 VersionFromString(OptionVersion));

		foreach (LabelResult result in results)
			{
				Console.WriteLine("{0} label {1}@{2}", result.Status, result.Label,
													result.Scope);
			}
	}
}
