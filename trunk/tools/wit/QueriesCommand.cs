//
// QueriesCommand.cs
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
using System.Reflection;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Mono.GetOptions;

[Command("queries", "Manage stored work item queries.", "")]
class QueriesCommand : Command
{
	public QueriesCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public override void Run()
	{
		WorkItemStore store = Driver.TeamFoundationServer.GetService(typeof(WorkItemStore)) as WorkItemStore;
		ILinking linking = Driver.TeamFoundationServer.GetService(typeof(ILinking)) as ILinking;
		int changeSet = 1;

		// Get URI for changeset
		ArtifactId changeSetId = new ArtifactId();
		changeSetId.Tool = "VersionControl";
		changeSetId.ArtifactType = "ChangeSet";
		changeSetId.ToolSpecificId = changeSet.ToString();
		string changeSetUri = LinkingUtilities.EncodeUri(changeSetId);

		// Get referencing artifacts for given changeset
		Artifact[] artifacts = linking.GetReferencingArtifacts(new string[] { changeSetUri }, null);

		foreach (Artifact artifact in artifacts)
			{
				Console.WriteLine(artifact.ToString());
				ArtifactId artifactId = LinkingUtilities.DecodeUri(artifact.Uri);
				if (String.Equals(artifactId.Tool, "WorkItemTracking", StringComparison.OrdinalIgnoreCase))
					{
						WorkItem wi = store.GetWorkItem(Convert.ToInt32(artifactId.ToolSpecificId));
						Console.WriteLine(wi);
					}
			}
	}
}