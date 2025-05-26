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
        ModuleMessageHandler.RegisterHandler<ArenaMenuMessage>();
        ModuleMessageHandler.RegisterHandler<ArenaTransitionMessage>();
        ModuleMessageHandler.RegisterHandler<ChallengeSelectMessage>();
        ModuleMessageHandler.RegisterHandler<GeoSelectMessage>();

        ModuleMessageHandler.RegisterHandler<BonelabHubEventMessage>();
        ModuleMessageHandler.RegisterHandler<DescentElevatorMessage>();
        ModuleMessageHandler.RegisterHandler<DescentIntroMessage>();
        ModuleMessageHandler.RegisterHandler<DescentNooseMessage>();
        ModuleMessageHandler.RegisterHandler<HomeEventMessage>();
        ModuleMessageHandler.RegisterHandler<KartRaceEventMessage>();
        ModuleMessageHandler.RegisterHandler<MagmaGateEventMessage>();
        ModuleMessageHandler.RegisterHandler<MineDiveCartMessage>();

        ModuleMessageHandler.RegisterHandler<BaseGameControllerMessage>();
        ModuleMessageHandler.RegisterHandler<HolodeckEventMessage>();
        ModuleMessageHandler.RegisterHandler<TimeTrialGameControllerMessage>();
        ModuleMessageHandler.RegisterHandler<TrialSpawnerEventsMessage>();

        ModuleMessageHandler.RegisterHandler<FlashlightToggleMessage>();
        ModuleMessageHandler.RegisterHandler<RandomObjectMessage>();
        ModuleMessageHandler.RegisterHandler<SimpleGripEventMessage>();
    }

    protected override void OnModuleUnregistered()
    {
        
    }
}