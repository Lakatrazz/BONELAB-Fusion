using LabFusion.Data;
using LabFusion.Preferences;

using Steamworks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public enum NetworkLayerType {
        STEAM = 0,
        STEAM_VR = 1,
        SPACEWAR = 2,
    }

    public static class NetworkLayerDeterminer {
        public static NetworkLayerType LoadedType { get; private set; }

        public static NetworkLayerType GetDefaultType() {
            return NetworkLayerType.STEAM;
        }

        public static NetworkLayerType VerifyType(NetworkLayerType type) {
            switch (type) {
                default:
                case NetworkLayerType.STEAM:
                    if (!SteamNetworkLayer.VerifyLayer())
                        return VerifyType(NetworkLayerType.STEAM_VR);
                    else
                        return NetworkLayerType.STEAM;
                case NetworkLayerType.STEAM_VR:
                    if (!SteamVRNetworkLayer.VerifyLayer())
                        return VerifyType(NetworkLayerType.SPACEWAR);
                    else
                        return NetworkLayerType.STEAM_VR;
                case NetworkLayerType.SPACEWAR:
                    return NetworkLayerType.SPACEWAR;
            }
        }

        public static Type GetLoadedType() {
            var type = FusionPreferences.ClientSettings.NetworkLayerType.GetValue();
            type = VerifyType(type);

            LoadedType = type;

            switch (type) {
                default:
                case NetworkLayerType.STEAM:
                    return typeof(SteamNetworkLayer);
                case NetworkLayerType.STEAM_VR:
                    return typeof(SteamVRNetworkLayer);
                case NetworkLayerType.SPACEWAR:
                    return typeof(SpacewarNetworkLayer);
            }
        }
    }
}
