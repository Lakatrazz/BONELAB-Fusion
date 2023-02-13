using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using UnityEngine;

using LabFusion.Extensions;

using MelonLoader;

using LabFusion.Utilities;

namespace LabFusion.Data
{
    public static class EmbeddedAssetBundle
    {
        public static AssetBundle LoadFromAssembly(Assembly assembly, string name)
        {
            string[] manifestResources = assembly.GetManifestResourceNames();

            if (manifestResources.Contains(name))
            {
                byte[] bytes = EmbeddedResource.LoadFromAssembly(assembly, name);

                FusionLogger.Log($"Loading bundle from data {name}, please be patient...", ConsoleColor.DarkCyan);
                var temp = AssetBundle.LoadFromMemory(bytes);
                FusionLogger.Log($"Done!", ConsoleColor.DarkCyan);
                return temp;
            }

            return null;
        }
    }
}
