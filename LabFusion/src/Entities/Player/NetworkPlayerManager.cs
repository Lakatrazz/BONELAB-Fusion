using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Data;

namespace LabFusion.Entities;

public static class NetworkPlayerManager
{
    private static readonly EntityUpdatableManager _updatableManager = new();
    public static EntityUpdatableManager UpdatableManager => _updatableManager;

    public static void OnInitializeManager()
    {
        // Reserve all player ids
        for (var i = PlayerIDManager.MinPlayerID; i <= PlayerIDManager.MaxPlayerID; i++)
        {
            NetworkEntityManager.IDManager.RegisteredEntities.ReserveID((ushort)i);
        }
    }

    public static bool HasExternalPlayer(RigManager rigManager)
    {
        if (!TryGetPlayer(rigManager, out var player))
        {
            return false;
        }

        return !player.NetworkEntity.IsOwner;
    }

    public static bool HasExternalPlayer(byte playerID)
    {
        if (!TryGetPlayer(playerID, out var player))
        {
            return false;
        }

        return !player.NetworkEntity.IsOwner;
    }

    public static bool HasPlayer(RigManager rigManager)
    {
        return NetworkPlayer.RigCache.ContainsSource(rigManager);
    }

    public static bool HasPlayer(byte playerID)
    {
        return NetworkEntityManager.IDManager.RegisteredEntities.HasEntity(playerID);
    }

    public static bool TryGetPlayer(byte playerID, out NetworkPlayer player)
    {
        player = null;

        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(playerID);

        if (entity == null)
        {
            return false;
        }

        player = entity.GetExtender<NetworkPlayer>();
        return player != null;
    }

    public static bool TryGetPlayer(RigManager rigManager, out NetworkPlayer player)
    {
        return NetworkPlayer.RigCache.TryGet(rigManager, out player);
    }

    public static bool TryGetPlayer(MarrowEntity marrowEntity, out NetworkPlayer player)
    {
        player = null;

        if (!IMarrowEntityExtender.Cache.TryGet(marrowEntity, out var networkEntity))
        {
            return false;
        }

        player = networkEntity.GetExtender<NetworkPlayer>();
        return player != null;
    }

    public static NetworkPlayer CreateLocalPlayer()
    {
        return CreateNetworkPlayer(PlayerIDManager.LocalID);
    }

    public static NetworkPlayer CreateNetworkPlayer(PlayerID playerID)
    {
        NetworkEntity networkEntity = new();
        NetworkPlayer networkPlayer = NetworkPlayer.CreatePlayer(networkEntity, playerID);

        NetworkEntityManager.IDManager.RegisterEntity(playerID.SmallID, networkEntity);

        return networkPlayer;
    }

    public static void OnUpdate(float deltaTime)
    {
        UpdatableManager.OnEntityUpdate(deltaTime);
    }

    public static void OnFixedUpdate(float deltaTime)
    {
        UpdatableManager.OnEntityFixedUpdate(deltaTime);
    }

    public static void OnLateUpdate(float deltaTime)
    {
        UpdatableManager.OnEntityLateUpdate(deltaTime);
    }
}
