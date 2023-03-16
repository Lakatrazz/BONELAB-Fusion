using BoneLib;

using LabFusion.Data;
using LabFusion.Debugging;
using LabFusion.Preferences;

using Steamworks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public enum NetworkLayerType {
        STEAM_VR = 0,
        SPACEWAR = 1,
        EMPTY = 2,
    }

    public static class NetworkLayerDeterminer {
        public static NetworkLayerType LoadedType { get; private set; }

        public static NetworkLayerType GetDefaultType() {
            if (HelperMethods.IsAndroid())
                return NetworkLayerType.EMPTY;

            return NetworkLayerType.STEAM_VR;
        }

        public static NetworkLayerType VerifyType(NetworkLayerType type) {
            switch (type) {
                default:
                case NetworkLayerType.STEAM_VR:
                    if (!SteamVRNetworkLayer.VerifyLayer())
                        return VerifyType(NetworkLayerType.SPACEWAR);
                    else
                        return NetworkLayerType.STEAM_VR;
                case NetworkLayerType.SPACEWAR:
                    if (!SpacewarNetworkLayer.VerifyLayer())
                        return VerifyType(NetworkLayerType.EMPTY);
                    else
                        return NetworkLayerType.SPACEWAR;
                case NetworkLayerType.EMPTY:
                    return NetworkLayerType.EMPTY;
            }
        }

        public static Type GetLoadedType() {
            var type = FusionPreferences.ClientSettings.NetworkLayerType.GetValue();
            type = VerifyType(type);

            LoadedType = type;

            switch (type) {
                default:
                case NetworkLayerType.STEAM_VR:
                    return typeof(SteamVRNetworkLayer);
                case NetworkLayerType.SPACEWAR:
                    return typeof(SpacewarNetworkLayer);
                case NetworkLayerType.EMPTY:
                    return typeof(EmptyNetworkLayer);
            }
        }
    }
}
