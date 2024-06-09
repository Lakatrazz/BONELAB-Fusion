using System.Reflection;

using UnityEngine;

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

        public static AssetBundleCreateRequest LoadFromAssemblyAsync(Assembly assembly, string name)
        {
            string[] manifestResources = assembly.GetManifestResourceNames();

            if (manifestResources.Contains(name))
            {
                byte[] bytes = EmbeddedResource.LoadFromAssembly(assembly, name);

                FusionLogger.Log($"Loading bundle from data {name} asynchronously.", ConsoleColor.DarkCyan);
                var request = AssetBundle.LoadFromMemoryAsync(bytes);

                request.add_completed((Il2CppSystem.Action<AsyncOperation>)((a) =>
                {
                    FusionLogger.Log($"Finished loading {name}!", ConsoleColor.DarkCyan);
                }));

                return request;
            }

            return null;
        }
    }
}
