using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using LabFusion.Marrow.Messages;

namespace LabFusion.Marrow;

public class MarrowModule : Module
{
    public override string Name => "Marrow";
    public override string Author => FusionMod.ModAuthor;
    public override Version Version => FusionMod.Version;

    public override ConsoleColor Color => ConsoleColor.White;

    protected override void OnModuleRegistered()
    {
        ModuleMessageHandler.RegisterHandler<ButtonChargeMessage>();
        ModuleMessageHandler.RegisterHandler<EventActuatorMessage>();

        ModuleMessageHandler.RegisterHandler<GunShotMessage>();
        ModuleMessageHandler.RegisterHandler<PuppetMasterKillMessage>();

        ModuleMessageHandler.RegisterHandler<ObjectDestructibleDestroyMessage>();

        ModuleMessageHandler.RegisterHandler<GamemodeDropperMessage>();

        MultiplayerHooking.OnMainSceneInitialized += NetworkGunManager.OnMainSceneInitialized;
    }

    protected override void OnModuleUnregistered()
    {
        
    }
}