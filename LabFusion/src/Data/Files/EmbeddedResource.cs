﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LabFusion.Utilities;

namespace LabFusion.Data
{
    public static class EmbeddedResource
    {
        public static byte[] LoadFromAssembly(Assembly assembly, string name)
        {
            string[] manifestResources = assembly.GetManifestResourceNames();

            if (manifestResources.Contains(name))
            {
                FusionLogger.Log($"Loading embedded resource data {name}...", ConsoleColor.DarkCyan);
                using Stream str = assembly.GetManifestResourceStream(name);
                using MemoryStream memoryStream = new();

                str.CopyTo(memoryStream);
                FusionLogger.Log("Done!", ConsoleColor.DarkCyan);

                return memoryStream.ToArray();
            }

            return null;
        }
    }
}
