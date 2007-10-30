//
// ShowCommand.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services.Protocols;
using Mono.GetOptions;

[System.Web.Services.WebServiceBinding(Name="AdminSoap", Namespace="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/Admin/03")]
public class AdminStats : System.Web.Services.Protocols.SoapHttpClientProtocol {
    
	public AdminStats(string url, NetworkCredential credentials) 
	{
		this.Url = url + "/VersionControl/v1.0/administration.asmx";
		this.Credentials = credentials;
	}
    
	[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/Admin/03/QueryRepositoryInformation", RequestNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/Admin/03", ResponseNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/Admin/03", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
  public AdminRepositoryInfo QueryRepositoryInformation() 
	{
		object[] results = this.Invoke("QueryRepositoryInformation", new object[0]);
		return ((AdminRepositoryInfo)(results[0]));
	}
}

[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/Admin/03")]
public class AdminRepositoryInfo {
    
    [System.Xml.Serialization.XmlAttributeAttribute(Namespace="")]
    public int UserCount;
    
    [System.Xml.Serialization.XmlAttributeAttribute(Namespace="")]
    public int GroupCount;
    
    [System.Xml.Serialization.XmlAttributeAttribute(Namespace="")]
    public int WorkspaceCount;
    
    [System.Xml.Serialization.XmlAttributeAttribute(Namespace="")]
    public int ShelvesetCount;
    
    [System.Xml.Serialization.XmlAttributeAttribute(Namespace="")]
    public int FileCount;
    
    [System.Xml.Serialization.XmlAttributeAttribute(Namespace="")]
    public int FolderCount;
    
    [System.Xml.Serialization.XmlAttributeAttribute(Namespace="")]
    public int MaxChangesetID;
    
    [System.Xml.Serialization.XmlAttributeAttribute(Namespace="")]
    public int PendingChangeCount;
    
    [System.Xml.Serialization.XmlAttributeAttribute(Namespace="")]
    public int ShelvesetDeletedCount;
}

[Command("show", "Show information about build configuration, cache settings, identity info, usage statistics, or registered tools.", "[ build | cache | ident | stats | tools ]")]
class ShowCommand : Command
{
	public ShowCommand(Driver driver, string[] args): base(driver, args)
	{
	}

	public void ShowStats()
	{
		AdminStats stats = new AdminStats(Driver.ServerUrl, Driver.GetNetworkCredentials());
		AdminRepositoryInfo info = stats.QueryRepositoryInformation();

		Console.WriteLine("Files:           " + info.FileCount);
		Console.WriteLine("Folders:         " + info.FolderCount);
		Console.WriteLine("Groups:          " + info.GroupCount);
		Console.WriteLine("Pending Changes: " + info.PendingChangeCount);
		Console.WriteLine("Shelvesets:      " + info.ShelvesetCount);
		Console.WriteLine("Users:           " + info.UserCount);
		Console.WriteLine("Workspaces:      " + info.WorkspaceCount);
	}

	public class ProjectInfoComparer: IComparer<ProjectInfo>
	{
    public int Compare(ProjectInfo x, ProjectInfo y)
    {
			return x.Name.CompareTo(y.Name);
    }
	}

	public void ShowProjects()
	{
		ICommonStructureService css = Driver.TeamFoundationServer.GetService(typeof(ICommonStructureService)) as ICommonStructureService;
		ProjectInfo[] projects = css.ListProjects();
		Array.Sort(projects, new ProjectInfoComparer());

		int GUID_SIZE = 36;
		int maxName = WindowWidth - GUID_SIZE - 2;
		string line = String.Format("{0} {1}", 
																"Guid".PadRight(GUID_SIZE), 
																"Name".PadRight(maxName));
		Console.WriteLine(line);
				
		line = String.Format("{0} {1}", 
												 "-".PadRight(GUID_SIZE, '-'),
												 "-".PadRight(maxName, '-'));

		Console.WriteLine(line);

		foreach (ProjectInfo pinfo in (projects))
			{
				int indx = Math.Max(pinfo.Uri.LastIndexOf('/') + 1, 0);
				string guid = pinfo.Uri.Substring(indx);
				Console.WriteLine(guid + " " + pinfo.Name);
			}
	}

	public void ShowTools()
	{
		RegistrationEntry[] registrationEntries = ((IRegistration) Driver.TeamFoundationServer.GetService(typeof(IRegistration))).GetRegistrationEntries("");
		foreach (RegistrationEntry entry in registrationEntries)
			{
				Console.WriteLine(entry.Type);
				Console.WriteLine(new String('-', WindowWidth));

				if (entry.ArtifactTypes.Length > 0)
					{
						Console.WriteLine();
						Console.WriteLine("ArtifactTypes: ");
						Console.WriteLine();
						foreach (ArtifactType artifactType in entry.ArtifactTypes)
							{
								Console.WriteLine("  {0}", artifactType.Name);
								foreach (OutboundLinkType outboundLinkType in artifactType.OutboundLinkTypes)
									{
										Console.WriteLine("    {0}: {1} {2}", outboundLinkType.Name,
																			outboundLinkType.TargetArtifactTypeName, 
																			outboundLinkType.TargetArtifactTypeTool);
									}
							}
					}

				if (entry.Databases.Length > 0)
					{
						Console.WriteLine();
						Console.WriteLine("Databases: ");
						Console.WriteLine();
						foreach (Database database in entry.Databases)
							{
								Console.WriteLine("  {0}: {1} on {2}", database.Name, database.DatabaseName, database.SQLServerName);
							}
					}

				Console.WriteLine();
				Console.WriteLine("ServiceInterfaces: ");
				Console.WriteLine();

				foreach (ServiceInterface serviceInterface in entry.ServiceInterfaces)
					{
						Console.WriteLine("  {0}: {1}", serviceInterface.Name, serviceInterface.Url);
					}

				Console.WriteLine();
			}
	}

	private void ShowCache()
	{
		Console.WriteLine("Cached Workspaces:");
		Console.WriteLine();

		WorkspaceInfo[] infos = Workstation.Current.GetAllLocalWorkspaceInfo();
		foreach (WorkspaceInfo info in infos)
			{
				Console.WriteLine("Name: " + info.Name);
				Console.WriteLine("Uri : " + info.ServerUri.ToString());
				Console.WriteLine();

				foreach (string path in info.MappedPaths)
					{
						Console.WriteLine("  Path: " + path);
					}

				Console.WriteLine();
			}
	}

	public void ShowBuild()
	{
		bool magic = false;
#if HAVE_MAGIC
		magic = true;
#endif
		Console.WriteLine("File Type Support:     {0}", magic);

		bool keyring = false;
#if HAVE_GNOME_KEYRING
		keyring = true;
#endif
		Console.WriteLine("Gnome Keyring Support: {0}", keyring);

		bool gui = false;
#if HAVE_GTK
		gui = true;
#endif
		Console.WriteLine("GUI Support:           {0}", gui);

		bool highlight = false;
#if HAVE_SYNTAX_HIGHLIGHTING
		highlight = true;
#endif
		Console.WriteLine("Syntax Highlighting:   {0}", highlight);
	}

	public void ShowIdentity()
	{
		IGroupSecurityService gss = Driver.TeamFoundationServer.GetService(typeof(IGroupSecurityService)) as IGroupSecurityService;
		Identity identity = gss.ReadIdentity(SearchFactor.AccountName, Driver.Username, QueryMembership.Direct);
		Console.WriteLine("Account Name: " + identity.AccountName);
		Console.WriteLine("Description: " + identity.Description);
		Console.WriteLine("DisplayName: " + identity.DisplayName);
		Console.WriteLine("Distinguished Name: " + identity.DistinguishedName);
		Console.WriteLine("Domain: " + identity.Domain);
		Console.WriteLine("Mail Address: " + identity.MailAddress);
		Console.WriteLine("Sid: " + identity.Sid);
		Console.WriteLine("Type: " + identity.Type);
	}

	public override void Run()
	{
		if (Arguments.Length < 1)
			{
				Console.WriteLine("Usage: tf show [ build | cache | ident | stats | tools ]");
				Environment.Exit((int)ExitCode.Failure);
			}

		switch (Arguments[0])
			{
			case "build":
				ShowBuild();
				break;
			case "cache":
				ShowCache();
				break;
			case "ident":
				ShowIdentity();
				break;
			case "projects":
				ShowProjects();
				break;
			case "stats":
				ShowStats();
				break;
			case "tools":
				ShowTools();
				break;
			default:
				Console.WriteLine("Unknown show option: '{0}'", Arguments[0]);
				break;
			}
	}
}
