using LabFusion.Extensions;
using LabFusion.Preferences.Server;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Marrow;
using LabFusion.Data;

using System.Text.Json;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Network;

public struct LobbyMetadataInfo
{
    // Lobby info
    public ulong LobbyId;
    public string LobbyOwner;
    public string LobbyName;
    public string LobbyDescription;
    public Version LobbyVersion;
    public bool HasServerOpen;
    public int PlayerCount;
    public PlayerList PlayerList;

    // Lobby settings
    public bool NametagsEnabled;
    public ServerPrivacy Privacy;
    public TimeScaleMode TimeScaleMode;
    public int MaxPlayers;
    public bool VoiceChatEnabled;

    // Lobby status
    public string LevelName;
    public string LevelBarcode;

    public string GamemodeName;
    public bool IsGamemodeRunning;

    public bool ClientHasLevel;

    public static LobbyMetadataInfo Create()
    {
        var playerList = new PlayerList();
        playerList.WritePlayers();

        return new LobbyMetadataInfo()
        {
            // Lobby info
            LobbyId = PlayerIdManager.LocalLongId,
            LobbyOwner = PlayerIdManager.LocalUsername,
            LobbyName = ServerSettingsManager.SavedSettings.ServerName.Value,
            LobbyDescription = ServerSettingsManager.SavedSettings.ServerDescription.Value,
            LobbyVersion = FusionMod.Version,
            HasServerOpen = NetworkInfo.IsServer,
            PlayerCount = PlayerIdManager.PlayerCount,
            PlayerList = playerList,

            // Lobby settings
            NametagsEnabled = ServerSettingsManager.SavedSettings.NametagsEnabled.Value,
            Privacy = ServerSettingsManager.SavedSettings.Privacy.Value,
            TimeScaleMode = ServerSettingsManager.SavedSettings.TimeScaleMode.Value,
            MaxPlayers = ServerSettingsManager.SavedSettings.MaxPlayers.Value,
            VoiceChatEnabled = ServerSettingsManager.SavedSettings.VoiceChatEnabled.Value,

            // Lobby status
            LevelName = FusionSceneManager.Title,
            LevelBarcode = FusionSceneManager.Barcode,
            GamemodeName = Gamemode.TargetGamemode != null ? Gamemode.TargetGamemode.GamemodeName : "No Gamemode",
            IsGamemodeRunning = Gamemode.IsGamemodeRunning,
        };
    }

    public void Write(INetworkLobby lobby)
    {
        // Lobby info
        lobby.SetMetadata(nameof(LobbyId), LobbyId.ToString());
        lobby.SetMetadata(nameof(LobbyOwner), LobbyOwner);
        lobby.SetMetadata(nameof(LobbyName), LobbyName);
        lobby.SetMetadata(nameof(LobbyDescription), LobbyDescription);
        lobby.SetMetadata(nameof(LobbyVersion), LobbyVersion.ToString());
        lobby.SetMetadata(LobbyConstants.HasServerOpenKey, HasServerOpen.ToString());
        lobby.SetMetadata(nameof(PlayerCount), PlayerCount.ToString());
        lobby.SetMetadata(nameof(PlayerList), JsonSerializer.Serialize(PlayerList));

        // Lobby settings
        lobby.SetMetadata(nameof(NametagsEnabled), NametagsEnabled.ToString());
        lobby.SetMetadata(nameof(Privacy), Privacy.ToString());
        lobby.SetMetadata(nameof(TimeScaleMode), TimeScaleMode.ToString());
        lobby.SetMetadata(nameof(MaxPlayers), MaxPlayers.ToString());
        lobby.SetMetadata(nameof(VoiceChatEnabled), VoiceChatEnabled.ToString());

        // Lobby status
        lobby.SetMetadata(nameof(LevelName), LevelName);
        lobby.SetMetadata(nameof(LevelBarcode), LevelBarcode);
        lobby.SetMetadata(nameof(GamemodeName), GamemodeName);
        lobby.SetMetadata(nameof(IsGamemodeRunning), IsGamemodeRunning.ToString());

        // Now, write all the keys into an array in the metadata
        lobby.WriteKeyCollection();
    }

    public static LobbyMetadataInfo Read(INetworkLobby lobby)
    {
        var info = new LobbyMetadataInfo()
        {
            // Lobby info
            LobbyOwner = lobby.GetMetadata(nameof(LobbyOwner)),
            LobbyName = lobby.GetMetadata(nameof(LobbyName)),
            LobbyDescription = lobby.GetMetadata(nameof(LobbyDescription)),
            HasServerOpen = lobby.GetMetadata(LobbyConstants.HasServerOpenKey) == bool.TrueString,

            // Lobby settings
            NametagsEnabled = lobby.GetMetadata(nameof(NametagsEnabled)) == bool.TrueString,
            VoiceChatEnabled = lobby.GetMetadata(nameof(VoiceChatEnabled)) == bool.TrueString,

            // Lobby status
            LevelName = lobby.GetMetadata(nameof(LevelName)),
            GamemodeName = lobby.GetMetadata(nameof(GamemodeName)),
            IsGamemodeRunning = lobby.GetMetadata(nameof(IsGamemodeRunning)) == bool.TrueString,
        };

        // Check if we have a player list
        if (lobby.TryGetMetadata(nameof(PlayerList), out var json))
        {
            try
            {
                info.PlayerList = JsonSerializer.Deserialize<PlayerList>(json);
            }
            catch
            {
                info.PlayerList = new()
                {
                    Players = Array.Empty<PlayerInfo>()
                };
            }
        }
        else
        {
            info.PlayerList = new()
            {
                Players = Array.Empty<PlayerInfo>()
            };
        }

        // Check if we have the level the host has
        if (lobby.TryGetMetadata(nameof(LevelBarcode), out var barcode))
        {
            info.LevelBarcode = barcode;
            info.ClientHasLevel = CrateFilterer.HasCrate<LevelCrate>(new(barcode));
        }
        else
        {
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

    public Action CreateJoinDelegate(INetworkLobby lobby)
    {
        // If the user does not have the host's level, it will automatically download
        // If it fails, the user will be disconnected
        // So, we no longer need to check if the client has the level here
        return lobby.CreateJoinDelegate(LobbyId);
    }
}

public static class LobbyMetadataHelper
{
    public static void WriteInfo(INetworkLobby lobby)
    {
        LobbyMetadataInfo.Create().Write(lobby);
    }

    public static LobbyMetadataInfo ReadInfo(INetworkLobby lobby)
    {
        try
        {
            return LobbyMetadataInfo.Read(lobby);
        }
        catch (Exception e)
        {
#if DEBUG
            FusionLogger.LogException("reading lobby info", e);
#endif

            return new LobbyMetadataInfo() { HasServerOpen = false };
        }
    }
}