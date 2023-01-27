using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using System.Linq;

using MelonLoader;

using LabFusion.Utilities;
using LabFusion.Network;

namespace LabFusion.SDK.Modules {
    public static class ModuleHandler {
        internal static readonly List<Module> _loadedModules = new List<Module>();

        internal static void Internal_HookAssemblies() {
            AppDomain.CurrentDomain.AssemblyLoad += Internal_AssemblyLoad;
        }

        internal static void Internal_UnhookAssemblies() {
            AppDomain.CurrentDomain.AssemblyLoad -= Internal_AssemblyLoad;
        }

        private static void Internal_AssemblyLoad(object sender, AssemblyLoadEventArgs args) {
            LoadModule(args.LoadedAssembly);
        }

        /// <summary>
        /// Searches for a module in an assembly and attempts to load it.
        /// </summary>
        /// <param name="moduleAssembly"></param>
        public static void LoadModule(Assembly moduleAssembly) {
            if (moduleAssembly != null) {
                var moduleInfo = moduleAssembly.GetCustomAttribute<ModuleInfo>();

                if (moduleInfo != null && moduleInfo.moduleType != null) {
                    ModuleMessageHandler.LoadHandlers(moduleAssembly);

                    Internal_SetupModule(moduleInfo);
                }
            }
        }

        private static void Internal_SetupModule(ModuleInfo info) {
            if (Activator.CreateInstance(info.moduleType) is Module module) {
                Internal_PrintDescription(info);

                _loadedModules.Add(module);
                module.ModuleLoaded(info);
            }
        }

        internal static void Internal_PrintDescription(ModuleInfo info) {
            FusionLogger.Log("--==== Loaded Fusion Module ====--", info.color);

            FusionLogger.Log($"{info.name} - v{info.version}");

            if (!string.IsNullOrWhiteSpace(info.abbreviation))
                FusionLogger.Log($"aka [{info.abbreviation}]");

            FusionLogger.Log($"by {info.author}");

            FusionLogger.Log("--=============================--", info.color);
        }
    }
}
