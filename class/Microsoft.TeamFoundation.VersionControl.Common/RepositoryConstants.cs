//
// Microsoft.TeamFoundation.VersionControl.Common.RepositoryConstants
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

namespace Microsoft.TeamFoundation.VersionControl.Common
{
	public class RepositoryConstants
	{
		public const string AuthenticatedUser =".";
		public const string Upload="Upload";
		public const string Download="Download";
		public const int EncodingBinary=-1;
		public const string WorkspaceNameField="wsname";
		public const string WorkspaceOwnerField="wsowner";
		public const string HashField = "hash";		
		public const string ServerItemField = "item";
		public const string RangeField = "range";
		public const string LengthField = "filelength";
		public const string ContentField = "content";

		// these are supposed to be internal, but i'm not sure yet how these values
		// are exposed to Microsoft.TeamFoundation.VersionControl.Client classes
		public const string UploadUrlSuffix = "VersionControl/v1.0/upload.asmx";
		public const string DownloadUrlSuffix = "VersionControl/v1.0/item.asmx";
	}
}
