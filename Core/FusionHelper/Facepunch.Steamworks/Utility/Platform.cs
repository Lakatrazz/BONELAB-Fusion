using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Steamworks
{
	internal static class Platform
    {
		public const int StructPlatformPackSize = 8;

#if PLATFORM_WIN
    public const string LibraryName = "steam_api64";
#elif PLATFORM_MAC || PLATFORM_LINUX
    public const string LibraryName = "libsteam_api";
#endif

        public const CallingConvention CC = CallingConvention.Cdecl;
		public const int StructPackSize = 4;
	}
}
