using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using UnityEngine;

using MelonLoader;

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
                using (Stream str = assembly.GetManifestResourceStream(name))
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    str.CopyTo(memoryStream);
                    FusionLogger.Log("Done!", ConsoleColor.DarkCyan);
                    return memoryStream.ToArray();
                }
            }

            return null;
        }
    }
}
