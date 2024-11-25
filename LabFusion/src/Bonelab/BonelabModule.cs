using LabFusion.SDK.Modules;

namespace LabFusion.Bonelab;

public class BonelabModule : Module
{
    public override string Name => "BONELAB";
    public override string Author => "Lakatrazz";
    public override Version Version => FusionMod.Version;

    public override ConsoleColor Color => ConsoleColor.Cyan;

    protected override void OnModuleRegistered()
    {
        ModuleMessageHandler.RegisterHandler<RandomObjectMessage>();
        ModuleMessageHandler.RegisterHandler<SimpleGripEventMessage>();
        ModuleMessageHandler.RegisterHandler<FlashlightToggleMessage>();
    }

    protected override void OnModuleUnregistered()
    {
        
    }
}