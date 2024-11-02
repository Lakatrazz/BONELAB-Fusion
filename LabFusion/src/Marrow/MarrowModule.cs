using LabFusion.Marrow.Patching;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

namespace LabFusion.Marrow;

public class MarrowModule : Module
{
    public override string Name => "Marrow";
    public override string Author => "Lakatrazz";
    public override Version Version => FusionMod.Version;

    public override ConsoleColor Color => ConsoleColor.White;

    protected override void OnModuleRegistered()
    {
        ModuleMessageHandler.RegisterHandler<ButtonChargeMessage>();
        ModuleMessageHandler.RegisterHandler<EventActuatorMessage>();

        MultiplayerHooking.OnMainSceneInitialized += NetworkGunManager.OnMainSceneInitialized;

        EventActuatorPatches.Patch();
    }

    protected override void OnModuleUnregistered()
    {
        
    }
}