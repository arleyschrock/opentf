//
// FileTypeDatabase.cs
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
using System.Runtime.InteropServices;
using Mono.Unix;

namespace OpenTF.Common
{
	public class FileTypeDatabase
	{
		[DllImport("magic")]
		private extern static IntPtr magic_open(int flags);

		[DllImport("magic")]
		private extern static void magic_close(IntPtr magic_cookie);

		[DllImport("magic")]
		private extern static IntPtr magic_file(IntPtr magic_cookie, string filename);

		[DllImport("magic")]
		private extern static int magic_load(IntPtr magic_cookie, IntPtr x);

		private static IntPtr _magic = IntPtr.Zero;

		static FileTypeDatabase()
		{
			_magic = magic_open(0);
			if (_magic == IntPtr.Zero)
				{
					Console.WriteLine("Warning: failed to open libmagic database");
					return;
				}

			magic_load(_magic, IntPtr.Zero);
		}

		~FileTypeDatabase()
		{
			if (_magic != IntPtr.Zero)
				magic_close(_magic);
		}

		public static bool ShouldBeExecutable(string filename)
		{
			if (_magic == IntPtr.Zero) return false;
			string desc = Marshal.PtrToStringAuto(magic_file(_magic, filename));
			if (String.IsNullOrEmpty(desc))
				{
					Console.WriteLine("Error querying file type for " + filename);
					return false;
				}

			return desc.Contains("executable");
		}
	}

	public class FileType
	{
		public static void MakeExecutable(string filename)
		{
			Mono.Unix.Native.Stat stat;
			if (0 == Mono.Unix.Native.Syscall.stat(filename, out stat))
				{
					Mono.Unix.Native.FilePermissions fp = stat.st_mode | Mono.Unix.Native.FilePermissions.S_IXUSR;
					Mono.Unix.Native.Syscall.chmod(filename, fp);
				}
		}
	}
}				
