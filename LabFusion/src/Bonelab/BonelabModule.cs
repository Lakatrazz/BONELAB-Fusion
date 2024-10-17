using LabFusion.SDK.Modules;

namespace LabFusion.Bonelab;

public class BonelabModule : Module
{
    public override string Name => "BONELAB";
    public override string Author => "Lakatrazz";
    public override string Version => FusionVersion.VersionString;

    public override ConsoleColor Color => ConsoleColor.Cyan;

    protected override void OnModuleRegistered()
    {
        ModuleMessageHandler.RegisterHandler<RandomObjectMessage>();
        ModuleMessageHandler.RegisterHandler<SimpleGripEventMessage>();
    }

    protected override void OnModuleUnregistered()
    {
        
    }
}