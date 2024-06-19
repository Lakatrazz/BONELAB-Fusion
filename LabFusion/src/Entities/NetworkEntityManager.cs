using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

namespace LabFusion.Entities;

public static class NetworkEntityManager
{
    private static readonly EntityIdManager<NetworkEntity> _idManager = new();
    public static EntityIdManager<NetworkEntity> IdManager => _idManager;

    private static readonly EntityUpdateList<IEntityUpdatable> _updateManager = new();
    public static EntityUpdateList<IEntityUpdatable> UpdateManager => _updateManager;

    private static readonly EntityUpdateList<IEntityFixedUpdatable> _fixedUpdateManager = new();
    public static EntityUpdateList<IEntityFixedUpdatable> FixedUpdateManager => _fixedUpdateManager;

    private static readonly EntityUpdateList<IEntityLateUpdatable> _lateUpdateManager = new();
    public static EntityUpdateList<IEntityLateUpdatable> LateUpdateManager => _lateUpdateManager;

    public static void OnInitializeManager()
    {
        MultiplayerHooking.OnPlayerCatchup += OnPlayerCatchup;
    }

    public static void OnCleanupIds()
    {
        IdManager.RegisteredEntities.ClearId();
        IdManager.QueuedEntities.ClearId();
    }

    public static void OnCleanupEntities()
    {
        // Clear registered entities
        var registeredEntities = IdManager.RegisteredEntities.EntityIdLookup.Keys.ToList();

        foreach (var entity in registeredEntities)
        {
            try
            {
                IdManager.UnregisterEntity(entity);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("unregistering NetworkEntity", e);
            }
        }

        IdManager.RegisteredEntities.Clear();

        // Clear queued entities
        var queuedEntities = IdManager.QueuedEntities.EntityIdLookup.Keys.ToList();

        foreach (var entity in queuedEntities)
        {
            try
            {
                entity.Unregister();
            }
            catch (Exception e)
            {
                FusionLogger.LogException("unregistering queued NetworkEntity", e);
            }
        }

        IdManager.QueuedEntities.Clear();
    }

    private static void OnPlayerCatchup(PlayerId playerId)
    {
        foreach (var entity in IdManager.RegisteredEntities.IdEntityLookup)
        {
            try
            {
                entity.Value.InvokeCatchup(playerId);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("sending catchup for NetworkEntity", e);
            }
        }
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
                FusionLogger.LogException("running entity Update", e);
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
                FusionLogger.LogException("running entity FixedUpdate", e);
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
                FusionLogger.LogException("running entity LateUpdate", e);
            }
        }
    }

    public static void RequestUnqueue(ushort queuedId)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        using var writer = FusionWriter.Create(EntityUnqueueRequestData.Size);
        var data = EntityUnqueueRequestData.Create(PlayerIdManager.LocalSmallId, queuedId);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.EntityUnqueueRequest, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }

    public static void TransferOwnership(NetworkEntity entity, PlayerId ownerId)
    {
        if (entity.IsOwnerLocked)
        {
            FusionLogger.Warn($"Attempted to transfer ownership for NetworkEntity at id {entity.Id}, but ownership was locked!");
            return;
        }

        using var writer = FusionWriter.Create(EntityPlayerData.Size);
        var request = EntityPlayerData.Create(ownerId.SmallId, entity.Id);
        writer.Write(request);

        using var message = FusionMessage.Create(NativeMessageTag.EntityOwnershipRequest, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }

    public static void TakeOwnership(NetworkEntity entity)
    {
        TransferOwnership(entity, PlayerIdManager.LocalId);
    }
}