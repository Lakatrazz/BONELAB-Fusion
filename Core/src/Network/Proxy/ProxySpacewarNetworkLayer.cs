namespace LabFusion.Network
{
    public sealed class ProxySpacewarNetworkLayer : ProxyNetworkLayer
    {
        public override uint ApplicationID => SpacewarNetworkLayer.SpacewarId;

        public override string Title => "Proxy Spacewar";
    }
}
