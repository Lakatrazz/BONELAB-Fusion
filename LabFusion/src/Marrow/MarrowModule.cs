using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using LabFusion.Marrow.Messages;
using LabFusion.Marrow.Combat;
using LabFusion.Marrow.Scene;

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

        ModuleMessageHandler.RegisterHandler<InventoryAmmoReceiverDropMessage>();
        ModuleMessageHandler.RegisterHandler<InventorySlotDropMessage>();
        ModuleMessageHandler.RegisterHandler<InventorySlotInsertMessage>();

        ModuleMessageHandler.RegisterHandler<ConstrainerModeMessage>();
        ModuleMessageHandler.RegisterHandler<ConstraintCreateMessage>();
        ModuleMessageHandler.RegisterHandler<ConstraintDeleteMessage>();
        ModuleMessageHandler.RegisterHandler<MagazineClaimMessage>();
        ModuleMessageHandler.RegisterHandler<MagazineEjectMessage>();
        ModuleMessageHandler.RegisterHandler<MagazineInsertMessage>();
        ModuleMessageHandler.RegisterHandler<NimbusGunNoClipMessage>();
        ModuleMessageHandler.RegisterHandler<ObjectDestructibleDestroyMessage>();

        ModuleMessageHandler.RegisterHandler<CrateSpawnerMessage>();

        MultiplayerHooking.OnMainSceneInitialized += NetworkGunManager.OnMainSceneInitialized;

        LevelEventHandler.OnInitializeMelon();
    }

    protected override void OnModuleUnregistered()
    {
        
    }
}