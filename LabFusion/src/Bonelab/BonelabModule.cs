using LabFusion.SDK.Modules;
using LabFusion.Bonelab.Messages;

namespace LabFusion.Bonelab;

public class BonelabModule : Module
{
    public override string Name => "BONELAB";
    public override string Author => FusionMod.ModAuthor;
    public override Version Version => FusionMod.Version;

    public override ConsoleColor Color => ConsoleColor.Cyan;

    protected override void OnModuleRegistered()
    {
        ModuleMessageManager.RegisterHandler<ArenaMenuMessage>();
        ModuleMessageManager.RegisterHandler<ArenaTransitionMessage>();
        ModuleMessageManager.RegisterHandler<ChallengeSelectMessage>();
        ModuleMessageManager.RegisterHandler<GeoSelectMessage>();

        ModuleMessageManager.RegisterHandler<BonelabHubEventMessage>();
        ModuleMessageManager.RegisterHandler<DescentElevatorMessage>();
        ModuleMessageManager.RegisterHandler<DescentIntroMessage>();
        ModuleMessageManager.RegisterHandler<DescentNooseMessage>();
        ModuleMessageManager.RegisterHandler<HomeEventMessage>();
        ModuleMessageManager.RegisterHandler<KartRaceEventMessage>();
        ModuleMessageManager.RegisterHandler<MagmaGateEventMessage>();
        ModuleMessageManager.RegisterHandler<MineDiveCartMessage>();

        ModuleMessageManager.RegisterHandler<BodyLogEffectMessage>();
        ModuleMessageManager.RegisterHandler<BodyLogToggleMessage>();

        ModuleMessageManager.RegisterHandler<BaseGameControllerMessage>();
        ModuleMessageManager.RegisterHandler<HolodeckEventMessage>();
        ModuleMessageManager.RegisterHandler<TimeTrialGameControllerMessage>();
        ModuleMessageManager.RegisterHandler<TrialSpawnerEventsMessage>();

        ModuleMessageManager.RegisterHandler<BoardGeneratorMessage>();
        ModuleMessageManager.RegisterHandler<FlashlightToggleMessage>();
        ModuleMessageManager.RegisterHandler<KeySlotMessage>();
        ModuleMessageManager.RegisterHandler<SimpleGripEventMessage>();
        ModuleMessageManager.RegisterHandler<SpawnGunSelectMessage>();

        ModuleMessageManager.RegisterHandler<RandomObjectMessage>();

        BonelabSpawnableReferences.RegisterBlacklist();
    }

    protected override void OnModuleUnregistered()
    {
        
    }
}