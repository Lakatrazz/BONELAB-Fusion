using LabFusion.Extensions;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.XML;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LabFusion.Network {
    public struct LobbyMetadataInfo {
        private const string _internalPrefix = "BONELAB_FUSION_";
        public const string HasServerOpenKey = _internalPrefix + "HasServerOpen";

        // Lobby info
        public ulong LobbyId;
        public string LobbyOwner;
        public string LobbyName;
        public string LobbyTags;
        public Version LobbyVersion;
        public bool HasServerOpen;
        public int PlayerCount;
        public PlayerList PlayerList;

        // Lobby settings
        public bool NametagsEnabled;
        public ServerPrivacy Privacy;
        public TimeScaleMode TimeScaleMode;
        public int MaxPlayers;
        public bool VoicechatEnabled;

        // Lobby status
        public string LevelName;
        public string LevelBarcode;
        public string GamemodeName;

        public bool ClientHasLevel;

        public static LobbyMetadataInfo Create() {
            var playerList = new PlayerList();
            playerList.ReadPlayerList();

            return new LobbyMetadataInfo() {
                // Lobby info
                LobbyId = PlayerIdManager.LocalLongId,
                LobbyOwner = PlayerIdManager.LocalUsername,
                LobbyName = FusionPreferences.LocalServerSettings.ServerName.GetValue(),
                LobbyTags = FusionPreferences.LocalServerSettings.ServerTags.GetValue().Contract(),
                LobbyVersion = FusionMod.Version,
                HasServerOpen = NetworkInfo.IsServer,
                PlayerCount = PlayerIdManager.PlayerCount,
                PlayerList = playerList,

                // Lobby settings
                NametagsEnabled = FusionPreferences.LocalServerSettings.NametagsEnabled.GetValue(),
                Privacy = FusionPreferences.LocalServerSettings.Privacy.GetValue(),
                TimeScaleMode = FusionPreferences.LocalServerSettings.TimeScaleMode.GetValue(),
                MaxPlayers = FusionPreferences.LocalServerSettings.MaxPlayers.GetValue(),
                VoicechatEnabled = FusionPreferences.LocalServerSettings.VoicechatEnabled.GetValue(),

                // Lobby status
                LevelName = FusionSceneManager.Title,
                LevelBarcode = FusionSceneManager.Barcode,
                GamemodeName = Gamemode.ActiveGamemode != null ? Gamemode.ActiveGamemode.GamemodeName : "No Gamemode",
            };
        }

        public void Write(INetworkLobby lobby) {
            // Lobby info
            lobby.SetMetadata(nameof(LobbyId), LobbyId.ToString());
            lobby.SetMetadata(nameof(LobbyOwner), LobbyOwner);
            lobby.SetMetadata(nameof(LobbyName), LobbyName);
            lobby.SetMetadata(nameof(LobbyTags), LobbyTags);
            lobby.SetMetadata(nameof(LobbyVersion), LobbyVersion.ToString());
            lobby.SetMetadata(HasServerOpenKey, HasServerOpen.ToString());
            lobby.SetMetadata(nameof(PlayerCount), PlayerCount.ToString());
            lobby.SetMetadata(nameof(PlayerList), PlayerList.WriteDocument().ToString());

            // Lobby settings
            lobby.SetMetadata(nameof(NametagsEnabled), NametagsEnabled.ToString());
            lobby.SetMetadata(nameof(Privacy), Privacy.ToString());
            lobby.SetMetadata(nameof(TimeScaleMode), TimeScaleMode.ToString());
            lobby.SetMetadata(nameof(MaxPlayers), MaxPlayers.ToString());
            lobby.SetMetadata(nameof(VoicechatEnabled), VoicechatEnabled.ToString());

            // Lobby status
            lobby.SetMetadata(nameof(LevelName), LevelName);
            lobby.SetMetadata(nameof(LevelBarcode), LevelBarcode);
            lobby.SetMetadata(nameof(GamemodeName), GamemodeName);
        }

        public static LobbyMetadataInfo Read(INetworkLobby lobby) {
            var info = new LobbyMetadataInfo() {
                // Lobby info
                LobbyOwner = lobby.GetMetadata(nameof(LobbyOwner)),
                LobbyName = lobby.GetMetadata(nameof(LobbyName)),
                LobbyTags = lobby.GetMetadata(nameof(LobbyTags)),
                HasServerOpen = lobby.GetMetadata(HasServerOpenKey) == bool.TrueString,

                // Lobby settings
                NametagsEnabled = lobby.GetMetadata(nameof(NametagsEnabled)) == bool.TrueString,
                VoicechatEnabled = lobby.GetMetadata(nameof(VoicechatEnabled)) == bool.TrueString,

                // Lobby status
                LevelName = lobby.GetMetadata(nameof(LevelName)),
                GamemodeName = lobby.GetMetadata(nameof(GamemodeName)),
            };
            // Check if we have a player list
            if (lobby.TryGetMetadata(nameof(PlayerList), out var playerXML)) {
                info.PlayerList = new PlayerList();
                info.PlayerList.ReadDocument(XDocument.Parse(playerXML));
            }
            else {
                info.PlayerList = new() {
                    players = new PlayerList.PlayerInfo[0]
                };
            }

            // Check if we have the level the host has
            if (lobby.TryGetMetadata(nameof(LevelBarcode), out var barcode)) {
                info.LevelBarcode = barcode;
                info.ClientHasLevel = FusionSceneManager.HasLevel(barcode);
            }
            else {
                // Incase the server is on a slightly older version without this feature, we just return true
                info.ClientHasLevel = true;
            }

            // Get version
            if (Version.TryParse(lobby.GetMetadata(nameof(LobbyVersion)), out var version))
                info.LobbyVersion = version;
            else
                info.LobbyVersion = new Version(0, 0, 0);

            // Get longs
            if (ulong.TryParse(lobby.GetMetadata(nameof(LobbyId)), out var lobbyId))
                info.LobbyId = lobbyId;

            // Get integers
            if (int.TryParse(lobby.GetMetadata(nameof(PlayerCount)), out int playerCount))
                info.PlayerCount = playerCount;

            if (int.TryParse(lobby.GetMetadata(nameof(MaxPlayers)), out int maxPlayers))
                info.MaxPlayers = maxPlayers;

            // Get enums
            if (Enum.TryParse(lobby.GetMetadata(nameof(Privacy)), out ServerPrivacy privacy))
                info.Privacy = privacy;

            if (Enum.TryParse(lobby.GetMetadata(nameof(TimeScaleMode)), out TimeScaleMode mode))
                info.TimeScaleMode = mode;

            return info;
        }
    }

    public static class LobbyMetadataHelper {
        public static void WriteInfo(INetworkLobby lobby) {
            LobbyMetadataInfo.Create().Write(lobby);
        }

        public static LobbyMetadataInfo ReadInfo(INetworkLobby lobby) {
            try {
                return LobbyMetadataInfo.Read(lobby);
            }
            catch {
                return new LobbyMetadataInfo() { HasServerOpen = false };
            }
        }
    }
}
