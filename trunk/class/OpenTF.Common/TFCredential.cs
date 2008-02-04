//
// TFCredential.cs
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
using System.Net;

namespace OpenTF.Common
{
	public class TFCredential : NetworkCredential
	{
		public TFCredential(string userinfo, string password)
			{
				ParseUserInfo(userinfo);
				Password = password;
				Console.WriteLine(UserName);
				Console.WriteLine(Password);
				Console.WriteLine(Domain);
			}

		public TFCredential(string login)
			{
				string userinfo;
				int comma = login.IndexOf(",");

				if (comma == -1) { ParseUserInfo(login); return; }
				Password = login.Substring(comma+1);

				ParseUserInfo(login.Substring(0, comma));
			}

		private void ParseUserInfo(string userinfo)
		{
			// try to find domain portion if given
			int slash = userinfo.IndexOf('\\');
			if (-1 != slash)
				{
					Domain = userinfo.Substring(0, slash);
					UserName = userinfo.Substring(slash+1);	
				}
			else
				{
					int atsign = userinfo.IndexOf('@');
					if (-1 != atsign)
						{
							UserName = userinfo.Substring(0, atsign);	
							Domain = userinfo.Substring(atsign+1);
						}
					else UserName = userinfo;
				}
		}
	}
}