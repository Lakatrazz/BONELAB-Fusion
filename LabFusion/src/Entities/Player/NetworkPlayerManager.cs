using Il2CppSLZ.Rig;

using LabFusion.Representation;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        for (var i = PlayerIdManager.MinPlayerId; i <= PlayerIdManager.MaxPlayerId; i++)
        {
            NetworkEntityManager.IdManager.RegisteredEntities.ReserveId((ushort)i);
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

    public static bool HasPlayer(byte playerId)
    {
        return NetworkEntityManager.IdManager.RegisteredEntities.HasEntity(playerId);
    }

    public static bool TryGetPlayer(byte playerId, out NetworkPlayer player)
    {
        player = null;

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(playerId);

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

    public static NetworkPlayer CreateLocalPlayer()
    {
        return CreateNetworkPlayer(PlayerIdManager.LocalId);
    }

    public static NetworkPlayer CreateNetworkPlayer(PlayerId playerId)
    {
        NetworkEntity networkEntity = new();
        NetworkPlayer networkPlayer = new(networkEntity, playerId);

        NetworkEntityManager.IdManager.RegisterEntity(playerId.SmallId, networkEntity);

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
