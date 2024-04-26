using System;
using System.Collections.Generic;
using System.Collections;

namespace LabFusion.Network
{
    public sealed class ProxySteamVRNetworkLayer : ProxyNetworkLayer
    {
        public override uint ApplicationID => SteamVRNetworkLayer.SteamVRId;

        public override string Title => "Proxy SteamVR";
    }
}
