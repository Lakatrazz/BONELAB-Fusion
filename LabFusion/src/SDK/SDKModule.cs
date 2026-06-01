using LabFusion.SDK.Messages;
using LabFusion.SDK.Modules;
using LabFusion.SDK.Wearables;

namespace LabFusion.SDK;

public sealed class SDKModule : Module
{
    public override string Name => "Fusion SDK";
    public override string Author => FusionMod.ModAuthor;
    public override Version Version => FusionMod.Version;

    public override ConsoleColor Color => ConsoleColor.White;

    protected override void OnModuleRegistered()
    {
        ModuleMessageManager.RegisterHandler<AnimationStateMessage>();
        ModuleMessageManager.RegisterHandler<GamemodeDropperMessage>();
        ModuleMessageManager.RegisterHandler<VoiceProxyInputMessage>();

        WearableManager.Initialize();

        WristWatchManager.Initialize();
    }
}
