//
// Microsoft.TeamFoundation.WorkItemTracking.Client.ClientService
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//
// Copyright (C) 2007 Joel Reed
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
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services.Protocols;
using Microsoft.TeamFoundation;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.ClientService
{
	[System.Web.Services.WebServiceBinding(Name="ClientServiceSoap", Namespace="http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03")]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(SecurityChange))]
	internal class ClientService : System.Web.Services.Protocols.SoapHttpClientProtocol {
		
		public ClientService(Uri url, ICredentials credentials) 
			{
				this.Url = String.Format("{0}/{1}", url, "WorkItemTracking/v1.0/ClientService.asmx");
				this.Credentials = credentials;

				CheckAuthentication();
			}

		public void CheckAuthentication() 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "CheckAuthentication");
			HttpWebResponse response = Invoke(msg);
			response.Close();
		}

		protected HttpWebResponse Invoke (Message message)
		{
			message.End();
			HttpWebResponse response = GetWebResponse(message.Request) as HttpWebResponse;

			if (response == null)
				{
					throw new TeamFoundationServerException("No response from server");
				}

			if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					string msg = String.Format("TF30063: You are not authorized to access {0} ({1}).\n--> Did you supply the correct username, password, and domain?", 
																		 (new Uri(this.Url)).Host, message.MethodName);

					//StreamReader readStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
					//					Console.Error.WriteLine (readStream.ReadToEnd ());
					//readStream.Close();

					throw new TeamFoundationServerException(msg); 
				}

			if (response.StatusCode == HttpStatusCode.InternalServerError)
				{
					//					throw new VersionControlException(GetExceptionMessage(response));
				}

			return response;
		}

		public string GetExceptionMessage(HttpWebResponse response)
		{
			StreamReader sr = new StreamReader(response.GetResponseStream(), new UTF8Encoding (false), false);
			XmlReader reader = new XmlTextReader(sr);
			string msg = String.Empty;
			while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "faultstring")
						{
							msg = reader.ReadElementContentAsString();
							break;
						}
				}

			response.Close();
			return msg;
		}
	}
}
