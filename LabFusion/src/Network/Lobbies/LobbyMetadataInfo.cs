using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Marrow;
using LabFusion.Utilities;

using System.Text.Json;

namespace LabFusion.Network;

public struct LobbyMetadataInfo
{
    public LobbyInfo LobbyInfo { get; set; }

    public bool HasServerOpen { get; set; }

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

        return new LobbyMetadataInfo()
        {
            LobbyInfo = lobbyInfo,
            HasServerOpen = NetworkInfo.IsHost,
            LobbyCode = lobbyInfo.LobbyCode,
            Privacy = lobbyInfo.Privacy,
            Full = lobbyInfo.PlayerCount >= lobbyInfo.MaxPlayers,
            VersionMajor = lobbyInfo.LobbyVersion.Major,
            VersionMinor = lobbyInfo.LobbyVersion.Minor,
            Game = GameHelper.GameName,
        };
    }

    public readonly void Write(INetworkLobby lobby)
    {
        lobby.SetMetadata(LobbyKeys.HasServerOpenKey, HasServerOpen.ToString());
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
            HasServerOpen = lobby.GetMetadata(LobbyKeys.HasServerOpenKey) == bool.TrueString,
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
                info.HasServerOpen = false;
            }
        }
        else
        {
            info.HasServerOpen = false;
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
