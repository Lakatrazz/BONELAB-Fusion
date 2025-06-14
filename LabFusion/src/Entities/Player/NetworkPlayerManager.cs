using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Entities;

public static class NetworkPlayerManager
{
    private static readonly EntityUpdateList<IEntityUpdatable> _updateManager = new();
    public static EntityUpdateList<IEntityUpdatable> UpdateManager => _updateManager;

    private static readonly EntityUpdateList<IEntityFixedUpdatable> _fixedUpdateManager = new();
    public static EntityUpdateList<IEntityFixedUpdatable> FixedUpdateManager => _fixedUpdateManager;

    private static readonly EntityUpdateList<IEntityLateUpdatable> _lateUpdateManager = new();
    public static EntityUpdateList<IEntityLateUpdatable> LateUpdateManager => _lateUpdateManager;

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

    public static bool HasExternalPlayer(byte playerId)
    {
        if (!TryGetPlayer(playerId, out var player))
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

    public static NetworkPlayer CreateNetworkPlayer(PlayerID playerId)
    {
        NetworkEntity networkEntity = new();
        NetworkPlayer networkPlayer = new(networkEntity, playerId);

        NetworkEntityManager.IDManager.RegisterEntity(playerId.SmallID, networkEntity);

        return networkPlayer;
    }

    public static void OnUpdate(float deltaTime)
    {
        foreach (var entity in UpdateManager.Entities)
        {
            try
            {
                entity.OnEntityUpdate(deltaTime);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("running player Update", e);
            }
        }
    }

    public static void OnFixedUpdate(float deltaTime)
    {
        foreach (var entity in FixedUpdateManager.Entities)
        {
            try
            {
                entity.OnEntityFixedUpdate(deltaTime);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("running player FixedUpdate", e);
            }
        }
    }

    public static void OnLateUpdate(float deltaTime)
    {
        foreach (var entity in LateUpdateManager.Entities)
        {
            try
            {
                entity.OnEntityLateUpdate(deltaTime);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("running player LateUpdate", e);
            }
        }
    }
}
