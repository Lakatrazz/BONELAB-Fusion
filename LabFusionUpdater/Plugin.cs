// Originally used for BoneLib
// https://github.com/yowchap/BoneLib/blob/main/BoneLib/BoneLibUpdater/Main.cs

using MelonLoader;
using MelonLoader.Utils;

using System.Drawing;
using System.IO;
using System.Reflection;

using static MelonLoader.MelonLogger;

namespace LabFusionUpdater;

public class FusionUpdaterPlugin : MelonPlugin
{
    public const string Name = "LabFusion Updater";
    public const string Author = "Lakatrazz";
    public const string Version = "1.1.0";

    public static FusionUpdaterPlugin Instance { get; private set; }
    public static Instance Logger { get; private set; }
    public static Assembly UpdaterAssembly { get; private set; }

    private MelonPreferences_Category _prefCategory = MelonPreferences.CreateCategory("LabFusionUpdater");
    private MelonPreferences_Entry<bool> _offlineModePref;

    public bool IsOffline => _offlineModePref.Value;

    public const string ModName = "LabFusion";
    public const string PluginName = "LabFusionUpdater";
    public const string FileExtension = ".dll";

    public static readonly string ModAssemblyPath = Path.Combine(MelonEnvironment.ModsDirectory, $"{ModName}{FileExtension}");
    public static readonly string PluginAssemblyPath = Path.Combine(MelonEnvironment.PluginsDirectory, $"{PluginName}{FileExtension}");

    public override void OnPreInitialization()
    {
        Instance = this;
        Logger = LoggerInstance;
        UpdaterAssembly = MelonAssembly.Assembly;

        _offlineModePref = _prefCategory.CreateEntry("OfflineMode", false);
        _prefCategory.SaveToFile(false);

        LoggerInstance.Msg(IsOffline ? Color.Yellow : Color.Green, IsOffline ? "Fusion Auto-Updater is OFFLINE." : "Fusion Auto-Updater is ONLINE.");

        if (IsOffline) 
        {
            if (!File.Exists(ModAssemblyPath)) 
            {
                LoggerInstance.Warning($"{ModName}{FileExtension} was not found in the Mods folder!");
                LoggerInstance.Warning("Download it from the Github or switch to ONLINE mode.");
                LoggerInstance.Warning("https://github.com/Lakatrazz/BONELAB-Fusion/releases");
            }
        }
        else 
        {
            Updater.UpdateMod();
        }
    }
}