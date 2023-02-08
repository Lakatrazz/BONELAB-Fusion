using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public struct LobbyMetadataInfo {
        // Lobby info
        public ulong LobbyId;
        public string LobbyName;
        public bool HasServerOpen;
        public int PlayerCount;

        // Lobby settings
        public ServerPrivacy Privacy;
        public TimeScaleMode TimeScaleMode;
        public bool NametagsEnabled;
        public int MaxPlayers;

        // Lobby status
        public string LevelName;

        public static LobbyMetadataInfo Create() {
            return new LobbyMetadataInfo() {
                // Lobby info
                LobbyId = PlayerIdManager.LocalLongId,
                LobbyName = PlayerIdManager.LocalUsername,
                HasServerOpen = NetworkInfo.IsServer,
                PlayerCount = PlayerIdManager.PlayerCount,

                // Lobby settings
                Privacy = FusionPreferences.ServerSettings.Privacy,
                TimeScaleMode = FusionPreferences.ServerSettings.TimeScaleMode,
                NametagsEnabled = FusionPreferences.ServerSettings.NametagsEnabled,
                MaxPlayers = 255,

                // Lobby status
                LevelName = LevelWarehouseUtilities.GetCurrentLevel().Title,
            };
        }

        public void Write(INetworkLobby lobby) {
            // Lobby info
            lobby.SetMetadata("LobbyId", LobbyId.ToString());
            lobby.SetMetadata("LobbyName", LobbyName);
            lobby.SetMetadata("HasServerOpen", HasServerOpen.ToString());
            lobby.SetMetadata("PlayerCount", PlayerCount.ToString());

            // Lobby settings
            lobby.SetMetadata("Privacy", Privacy.ToString());
            lobby.SetMetadata("TimeScaleMode", TimeScaleMode.ToString());
            lobby.SetMetadata("NametagsEnabled", NametagsEnabled.ToString());
            lobby.SetMetadata("MaxPlayers", MaxPlayers.ToString());

            // Lobby status
            lobby.SetMetadata("LevelName", LevelName);
        }

        public static LobbyMetadataInfo Read(INetworkLobby lobby) {
            var info = new LobbyMetadataInfo() {
                // Lobby info
                LobbyId = ulong.Parse(lobby.GetMetadata("LobbyId")),
                LobbyName = lobby.GetMetadata("LobbyName"),
                HasServerOpen = lobby.GetMetadata("HasServerOpen") == bool.TrueString,

                // Lobby settings
                NametagsEnabled = lobby.GetMetadata("NametagsEnabled") == bool.TrueString,

                // Lobby status
                LevelName = lobby.GetMetadata("LevelName"),
            };
            // Get integers
            if (int.TryParse(lobby.GetMetadata("PlayerCount"), out int playerCount))
                info.PlayerCount = playerCount;

            if (int.TryParse(lobby.GetMetadata("MaxPlayers"), out int maxPlayers))
                info.MaxPlayers = maxPlayers;

            // Get enums
            if (Enum.TryParse(lobby.GetMetadata("Privacy"), out ServerPrivacy privacy))
                info.Privacy = privacy;

            if (Enum.TryParse(lobby.GetMetadata("TimeScaleMode"), out TimeScaleMode mode))
                info.TimeScaleMode = mode;

            return info;
        }
    }

    public static class LobbyMetadataHelper {
        public static void WriteInfo(INetworkLobby lobby) {
            LobbyMetadataInfo.Create().Write(lobby);
        }

        public static LobbyMetadataInfo ReadInfo(INetworkLobby lobby) {
            return LobbyMetadataInfo.Read(lobby);
        }
    }
}
