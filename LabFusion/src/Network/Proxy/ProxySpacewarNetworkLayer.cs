namespace LabFusion.Network
{
    public sealed class ProxySpacewarNetworkLayer : ProxyNetworkLayer
    {
        public override uint ApplicationID => SpacewarNetworkLayer.SpacewarId;

        internal override string Title => "Proxy Spacewar";
    }
}
