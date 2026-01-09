using LabFusion.SDK.Modules;

namespace LabFusion.Network;

public class NetworkLayerModule : Module
{
    public override string Name => "Network Layers";
    public override string Author => FusionMod.ModAuthor;
    public override Version Version => FusionMod.Version;

    public override ConsoleColor Color => ConsoleColor.Green;

    protected override void OnModuleRegistered()
    {
        NetworkLayer.RegisterLayersFromAssembly(FusionMod.FusionAssembly);
    }
}
