// Originally used for BoneLib
// https://github.com/yowchap/BoneLib/blob/main/BoneLib/BoneLibUpdater/Main.cs

using MelonLoader;

using System;
using System.IO;
using System.Reflection;

using static MelonLoader.MelonLogger;

namespace LabFusionUpdater
{
    public struct FusionUpdaterVersion
    {
        public const byte versionMajor = 1;
        public const byte versionMinor = 0;
        public const short versionPatch = 0;
    }

    public class FusionUpdaterPlugin : MelonPlugin
    {
        public const string Name = "LabFusion Updater";
        public const string Author = "Lakatrazz";
        public static readonly Version Version = new Version(FusionUpdaterVersion.versionMajor, FusionUpdaterVersion.versionMinor, FusionUpdaterVersion.versionPatch);

        public static FusionUpdaterPlugin Instance { get; private set; }
        public static Instance Logger { get; private set; }
        public static Assembly UpdaterAssembly { get; private set; }

        private MelonPreferences_Category _prefCategory = MelonPreferences.CreateCategory("LabFusionUpdater");
        private MelonPreferences_Entry<bool> _offlineModePref;

        public bool IsOffline => _offlineModePref.Value;

        public const string ModName = "LabFusion";
        public const string PluginName = "LabFusionUpdater";
        public const string FileExtension = ".dll";

        public static readonly string ModAssemblyPath = Path.Combine(MelonHandler.ModsDirectory, $"{ModName}{FileExtension}");
        public static readonly string PluginAssemblyPath = Path.Combine(MelonHandler.PluginsDirectory, $"{PluginName}{FileExtension}");

        public override void OnPreInitialization()
        {
            Instance = this;
            Logger = LoggerInstance;
            UpdaterAssembly = MelonAssembly.Assembly;

            _offlineModePref = _prefCategory.CreateEntry("OfflineMode", false);
            _prefCategory.SaveToFile(false);

            LoggerInstance.Msg(IsOffline ? ConsoleColor.Yellow : ConsoleColor.Green, IsOffline ? "Fusion Auto-Updater is OFFLINE." : "Fusion Auto-Updater is ONLINE.");

            if (IsOffline) {
                if (!File.Exists(ModAssemblyPath)) {
                    LoggerInstance.Warning($"{ModName}{FileExtension} was not found in the Mods folder!");
                    LoggerInstance.Warning("Download it from the Github or switch to ONLINE mode.");
                    LoggerInstance.Warning("https://github.com/Lakatrazz/BONELAB-Fusion/releases");
                }
            }
            else {
                Updater.UpdateMod();
            }
        }

        public override void OnApplicationQuit() {
            Updater.UpdatePlugin();
        }
    }
}