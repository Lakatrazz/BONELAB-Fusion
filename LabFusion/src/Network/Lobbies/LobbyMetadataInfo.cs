using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Marrow;
using LabFusion.Support;

using System.Text.Json;

namespace LabFusion.Network;

public struct LobbyMetadataInfo
{
    public static readonly LobbyMetadataInfo Empty = new()
    {
        LobbyInfo = null,
        HasLobbyOpen = false,
        ClientHasLevel = false,
        LobbyCode = null,
        Privacy = ServerPrivacy.PUBLIC,
        Full = false,
        VersionMajor = 0,
        VersionMinor = 0,
        Game = null,
    };

    public LobbyInfo LobbyInfo { get; set; }

    public bool HasLobbyOpen { get; set; }

    public bool ClientHasLevel { get; set; }

    public string LobbyCode { get; set; }

    public ServerPrivacy Privacy { get; set; }

    public bool Full { get; set; }

    public int VersionMajor { get; set; }

    public int VersionMinor { get; set; }

    public string Game { get; set; }

    public static LobbyMetadataInfo Create()
    {
        var lobbyInfo = LobbyInfoManager.LobbyInfo;

        if (lobbyInfo == null)
        {
            return Empty;
        }

        return new LobbyMetadataInfo()
        {
            LobbyInfo = lobbyInfo,
            HasLobbyOpen = NetworkInfo.IsHost,
            LobbyCode = lobbyInfo.LobbyCode,
            Privacy = lobbyInfo.Privacy,
            Full = lobbyInfo.PlayerCount >= lobbyInfo.MaxPlayers,
            VersionMajor = lobbyInfo.LobbyVersion.Major,
            VersionMinor = lobbyInfo.LobbyVersion.Minor,
            Game = Support.GameInfo.GameName,
        };
    }

    public readonly void Write(INetworkLobby lobby)
    {
        lobby.SetMetadata(LobbyKeys.IdentifierKey, bool.TrueString);
        lobby.SetMetadata(LobbyKeys.HasLobbyOpenKey, HasLobbyOpen.ToString());
        lobby.SetMetadata(LobbyKeys.LobbyCodeKey, LobbyCode?.ToUpper());
        lobby.SetMetadata(LobbyKeys.PrivacyKey, ((int)Privacy).ToString());
        lobby.SetMetadata(LobbyKeys.FullKey, Full.ToString());
        lobby.SetMetadata(LobbyKeys.VersionMajorKey, VersionMajor.ToString());
        lobby.SetMetadata(LobbyKeys.VersionMinorKey, VersionMinor.ToString());
        lobby.SetMetadata(LobbyKeys.GameKey, Game);
        lobby.SetMetadata(nameof(LobbyInfo), JsonSerializer.Serialize(LobbyInfo));

        // Now, write all the keys into an array in the metadata
        lobby.WriteKeyCollection();
    }

    public static LobbyMetadataInfo Read(INetworkLobby lobby)
    {
        var info = new LobbyMetadataInfo()
        {
            HasLobbyOpen = lobby.GetMetadata(LobbyKeys.HasLobbyOpenKey) == bool.TrueString,
            LobbyCode = lobby.GetMetadata(LobbyKeys.LobbyCodeKey),
            Game = lobby.GetMetadata(LobbyKeys.GameKey),
            Full = lobby.GetMetadata(LobbyKeys.FullKey) == bool.TrueString,
        };

        if (lobby.TryGetMetadata(LobbyKeys.PrivacyKey, out var rawPrivacy) && int.TryParse(rawPrivacy, out var privacyInt)) 
        {
            info.Privacy = (ServerPrivacy)privacyInt;
        }

        if (lobby.TryGetMetadata(LobbyKeys.VersionMajorKey, out var rawVersionMajor) && int.TryParse(rawVersionMajor, out var versionMajorInt))
        {
            info.VersionMajor = versionMajorInt;
        }

        if (lobby.TryGetMetadata(LobbyKeys.VersionMinorKey, out var rawVersionMinor) && int.TryParse(rawVersionMinor, out var versionMinorInt))
        {
            info.VersionMinor = versionMinorInt;
        }

        // Check if we can get the main lobby info
        if (lobby.TryGetMetadata(nameof(LobbyInfo), out var json))
        {
            try
            {
                info.LobbyInfo = JsonSerializer.Deserialize<LobbyInfo>(json);
            }
            catch
            {
                info.HasLobbyOpen = false;
            }
        }
        else
        {
            info.HasLobbyOpen = false;
        }

        // Check if we have the level the host has
        info.ClientHasLevel = CrateFilterer.HasCrate<LevelCrate>(new(info.LobbyInfo.LevelBarcode));

        return info;
    }

    public readonly Action CreateJoinDelegate(INetworkLobby lobby)
    {
        // If the user does not have the host's level, it will automatically download
        // If it fails, the user will be disconnected
        // So, we no longer need to check if the client has the level here
        return lobby.CreateJoinDelegate(LobbyInfo.LobbyID);
    }
}
