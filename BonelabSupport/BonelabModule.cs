using LabFusion;
using LabFusion.Entities;
using LabFusion.Marrow.Scene;
using LabFusion.SDK.Achievements;
using LabFusion.SDK.Modules;

using MarrowFusion.Bonelab.Extenders;
using MarrowFusion.Bonelab.SDK;

using System.Reflection;

using Module = LabFusion.SDK.Modules.Module;

namespace MarrowFusion.Bonelab;

public class BonelabModule : Module
{
    public static BonelabModule Instance { get; private set; } = null;

    public static ModuleLogger Logger { get; private set; } = null;

    public static Assembly ModuleAssembly { get; private set; } = null;

    public static HarmonyLib.Harmony HarmonyInstance { get; private set; } = null;

    public override string Name => "BONELAB";
    public override string Author => FusionMod.ModAuthor;
    public override Version Version => FusionMod.Version;

    public override ConsoleColor Color => ConsoleColor.Cyan;

    protected override void OnModuleRegistered()
    {
        Instance = this;
        Logger = LoggerInstance;
        ModuleAssembly = Assembly.GetExecutingAssembly();

        HarmonyInstance = new(ModuleAssembly.FullName);
        HarmonyInstance.PatchAll(ModuleAssembly);

        ModuleMessageManager.LoadHandlers(ModuleAssembly);
        LevelEventHandler.LoadHandlers(ModuleAssembly);
        EntityComponentManager.LoadComponents(ModuleAssembly);
        AchievementManager.LoadAchievements(ModuleAssembly);

        BonelabSpawnableReferences.RegisterBlacklist();

        BonelabPlayerManager.Initialize();
        BonelabBitMartManager.Initialize();
        BonelabMusicManager.Initialize();
        BonelabBacklotManager.Initialize();
    }

    protected override void OnModuleUnregistered()
    {
        BonelabPlayerManager.Uninitialize();
        BonelabBitMartManager.Uninitialize();
    }
}