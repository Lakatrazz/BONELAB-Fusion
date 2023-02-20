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
        SPACEWAR = 1,
    }

    public static class NetworkLayerDeterminer {
        public static NetworkLayerType GetDefaultType() {
            return NetworkLayerType.STEAM;
        }

        public static NetworkLayerType VerifyType(NetworkLayerType type) {
            switch (type) {
                default:
                case NetworkLayerType.STEAM:
                    if (!SteamNetworkLayer.VerifyLayer())
                        return NetworkLayerType.SPACEWAR;
                    else
                        return NetworkLayerType.STEAM;
                case NetworkLayerType.SPACEWAR:
                    return NetworkLayerType.SPACEWAR;
            }
        }

        public static Type GetLoadedType() {
            var type = FusionPreferences.ClientSettings.NetworkLayerType.GetValue();
            type = VerifyType(type);

            switch (type) {
                default:
                case NetworkLayerType.STEAM:
                    return typeof(SteamNetworkLayer);
                case NetworkLayerType.SPACEWAR:
                    return typeof(SpacewarNetworkLayer);
            }
        }
    }
}
