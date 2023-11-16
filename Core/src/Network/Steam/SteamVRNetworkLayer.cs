using System;
using System.Collections.Generic;

namespace LabFusion.Network
{
    public class SteamVRNetworkLayer : SteamNetworkLayer
    {
        public const int SteamVRId = 250820;

        public override uint ApplicationID => SteamVRId;

        internal override string Title => "SteamVR";

        internal override bool TryGetFallback(out NetworkLayer fallback)
        {
            fallback = GetLayer<SpacewarNetworkLayer>();
            return fallback != null;
        }
    }
}
