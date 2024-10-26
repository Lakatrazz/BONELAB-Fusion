using LabFusion.SDK.Modules;

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
    }

    protected override void OnModuleUnregistered()
    {
        
    }
}