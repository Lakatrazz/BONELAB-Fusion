using LabFusion.Data;
using Steamworks;
using System;
using System.Collections.Generic;

namespace LabFusion.Network
{
    public class SpacewarNetworkLayer : SteamNetworkLayer {
        public const int SpacewarId = 480;

        public override uint ApplicationID => SpacewarId;

        // Verification method to see if our game can actually run this layer
        public static bool VerifyLayer()
        {
            // Make sure the API actually loaded
            if (!SteamAPILoader.HasSteamAPI)
                return false;

            try
            {
                // Try loading the steam client
                if (!SteamClient.IsValid)
                    SteamClient.Init(SpacewarId, AsyncCallbacks);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
