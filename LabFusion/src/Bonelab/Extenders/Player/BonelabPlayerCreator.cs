using LabFusion.Entities;

namespace LabFusion.Bonelab.Extenders;

public static class BonelabPlayerCreator
{
    public static void HookNetworkPlayer()
    {
        NetworkPlayer.OnNetworkPlayerRegistered += OnNetworkPlayerRegistered;
    }

    public static void UnhookNetworkPlayer()
    {
        NetworkPlayer.OnNetworkPlayerRegistered -= OnNetworkPlayerRegistered;
    }

    private static void OnNetworkPlayerRegistered(NetworkPlayer player)
    {
        BonelabNetworkPlayer.CreatePlayer(player.NetworkEntity, player);
    }
}
