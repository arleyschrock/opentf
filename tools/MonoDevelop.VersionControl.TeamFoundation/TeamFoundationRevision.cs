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
	class TfRevision : MonoDevelop.VersionControl.Revision
	{
		private string id;

		public TfRevision (Repository repo, Changeset changeSet): 
			base (repo, changeSet.CreationDate, changeSet.Owner, changeSet.Comment, null)
			{
				id = Convert.ToString(changeSet.ChangesetId);
				List<RevisionPath> paths = new List<RevisionPath>();
				foreach (Change change in changeSet.Changes)
					{
						paths.Add(new RevisionPath(change.Item.ServerItem, 
																			 RevisionActionFromChangeType(change.ChangeType),
																			 change.ChangeType.ToString())); 
					}

				ChangedFiles = paths.ToArray();
			}

		public override Revision GetPrevious()
		{
			Console.WriteLine("TeamFoundationRevision.cs: GetPrevious not implemented");
			return null;
		}

		public override string ToString()
		{
			return id;
		}

		private RevisionAction RevisionActionFromChangeType(ChangeType c)
		{
			switch (c) {
			case ChangeType.Add:
				return RevisionAction.Add;
			case ChangeType.Delete:
				return RevisionAction.Delete;
			case ChangeType.Rename:
				return RevisionAction.Replace;
			default:
				return RevisionAction.Other;
			}
		}
	}
}