using Il2CppSLZ.Bonelab;

using LabFusion.Bonelab.Data;
using LabFusion.Bonelab.Messages;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Player;

namespace LabFusion.Bonelab.Extenders;

public static class BonelabPlayerManager
{
    public static void Initialize()
    {
        NetworkPlayer.OnNetworkPlayerRegistered += OnNetworkPlayerRegistered;
        MultiplayerHooking.OnMainSceneInitialized += OnMainSceneInitialized;
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoined;
        MultiplayerHooking.OnJoinedServer += OnJoinedServer;
    }

    public static void Uninitialize()
    {
        NetworkPlayer.OnNetworkPlayerRegistered -= OnNetworkPlayerRegistered;
        MultiplayerHooking.OnMainSceneInitialized -= OnMainSceneInitialized;
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoined;
        MultiplayerHooking.OnJoinedServer -= OnJoinedServer;
    }

    private static void OnNetworkPlayerRegistered(NetworkPlayer player)
    {
        BonelabNetworkPlayer.CreatePlayer(player.NetworkEntity, player);
    }

    private static void OnMainSceneInitialized()
    {
        PlayerRefs.Instance.PlayerBodyVitals.rescaleEvent += (BodyVitals.RescaleUI)OnVitalsChanged;
    }

    private static void OnPlayerJoined(PlayerID playerID) => OnVitalsChanged();

    private static void OnJoinedServer() => OnVitalsChanged();

    private static void OnVitalsChanged()
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var bodyVitals = new SerializedBodyVitals(PlayerRefs.Instance.PlayerBodyVitals);

        MessageRelay.RelayModule<BodyVitalsMessage, SerializedBodyVitals>(bodyVitals, CommonMessageRoutes.ReliableToOtherClients);
    }
}
