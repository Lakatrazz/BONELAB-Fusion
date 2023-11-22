using System;
using System.Linq;
using System.Reflection;
using LabFusion.Utilities;
using UnityEngine;

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
                FusionLogger.Log("Done!", ConsoleColor.DarkCyan);
                return temp;
            }

            return null;
        }
    }
}
