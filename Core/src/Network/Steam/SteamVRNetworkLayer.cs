using LabFusion.Data;
using LabFusion.Utilities;
using Steamworks;
using System;
using System.Collections.Generic;

namespace LabFusion.Network
{
    public class SteamVRNetworkLayer : SteamNetworkLayer {
        public const int SteamVRId = 250820;

        public override uint ApplicationID => SteamVRId;

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
                    SteamClient.Init(SteamVRId, AsyncCallbacks);

                return true;
            }
            catch (Exception e)
            {
                FusionLogger.LogException("initializing SteamVR layer", e);
                return false;
            }
        }
    }
}
