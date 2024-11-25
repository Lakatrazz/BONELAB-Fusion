using LabFusion.Utilities;
using LabFusion.Marrow;
using LabFusion.Data;

using System.Text.Json;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Network;

public struct LobbyMetadataInfo
{
    public LobbyInfo LobbyInfo { get; set; }

    public bool HasServerOpen { get; set; }

    public bool ClientHasLevel { get; set; }

    public static LobbyMetadataInfo Create()
    {
        return new LobbyMetadataInfo()
        {
            LobbyInfo = LobbyInfoManager.LobbyInfo,
            HasServerOpen = NetworkInfo.IsServer,
        };
    }

    public void Write(INetworkLobby lobby)
    {
        lobby.SetMetadata(LobbyConstants.HasServerOpenKey, HasServerOpen.ToString());
        lobby.SetMetadata(nameof(LobbyInfo), JsonSerializer.Serialize(LobbyInfo));

        // Now, write all the keys into an array in the metadata
        lobby.WriteKeyCollection();
    }

    public static LobbyMetadataInfo Read(INetworkLobby lobby)
    {
        var info = new LobbyMetadataInfo()
        {
            HasServerOpen = lobby.GetMetadata(LobbyConstants.HasServerOpenKey) == bool.TrueString,
        };

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

    public Action CreateJoinDelegate(INetworkLobby lobby)
    {
        // If the user does not have the host's level, it will automatically download
        // If it fails, the user will be disconnected
        // So, we no longer need to check if the client has the level here
        return lobby.CreateJoinDelegate(LobbyInfo.LobbyId);
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