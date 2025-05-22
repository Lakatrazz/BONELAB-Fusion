namespace LabFusion.Network.Proxy;

public sealed class ProxySteamVRNetworkLayer : ProxyNetworkLayer
{
    public override uint ApplicationID => SteamVRNetworkLayer.SteamVRId;

    public override string Title => "Proxy SteamVR";

    public override string Platform => "Steam";
}
