using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.Utilities;

using MelonLoader;

using System.Collections;

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

    private static readonly EntityValidationList _ownershipTransferValidators = new();
    public static EntityValidationList OwnershipTransferValidators => _ownershipTransferValidators;

    private static readonly Dictionary<ushort, List<NetworkEntityDelegate>> _entityRegisteredCallbacks = new();

    public static void OnInitializeManager()
    {
        CatchupManager.OnPlayerServerCatchup += OnPlayerServerCatchup;
        IdManager.OnEntityRegistered += OnEntityRegistered;
    }

    public static void OnCleanupIds()
    {
        IdManager.RegisteredEntities.ClearId();
        IdManager.QueuedEntities.ClearId();

        _entityRegisteredCallbacks.Clear();
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

        _entityRegisteredCallbacks.Clear();
    }

    private static void OnEntityRegistered(NetworkEntity entity)
    {
        var id = entity.Id;

        if (_entityRegisteredCallbacks.TryGetValue(id, out var callbacks))
        {
            foreach (var callback in callbacks)
            {
                try
                {
                    callback(entity);
                }
                catch (Exception e)
                {
                    FusionLogger.LogException("executing NetworkEntityManager.EntityRegisteredCallback", e);
                }
            }

            _entityRegisteredCallbacks.Remove(id);
        }
    }

    private static void OnPlayerServerCatchup(PlayerId playerId)
    {
        MelonCoroutines.Start(SendCreationCatchupCoroutine(playerId));
    }

    private static IEnumerator SendCreationCatchupCoroutine(PlayerId playerId)
    {
        var catchupQueue = new Queue<NetworkEntity>(IdManager.RegisteredEntities.IdEntityLookup.Values);

        while (catchupQueue.Count > 0 && !FusionSceneManager.IsLoading() && playerId.IsValid)
        {
            var entity = catchupQueue.Dequeue();

            if (!entity.IsRegistered)
            {
                continue;
            }

            bool sent = SendCreationCatchup(entity, playerId);

            if (!sent)
            {
                continue;
            }

            yield return null;

            yield return null;
        }
    }

    private static bool SendCreationCatchup(NetworkEntity entity, PlayerId playerId)
    {
        try
        {
            return entity.InvokeCreationCatchup(playerId);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("sending creation catchup for NetworkEntity", e);
        }

        return false;
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

        var data = EntityUnqueueRequestData.Create(PlayerIdManager.LocalSmallId, queuedId);

        MessageRelay.RelayNative(data, NativeMessageTag.EntityUnqueueRequest, NetworkChannel.Reliable, RelayType.ToServer);
    }

    public static void TransferOwnership(NetworkEntity entity, PlayerId ownerId)
    {
        if (!OwnershipTransferValidators.Validate(entity, ownerId))
        {
#if DEBUG
            FusionLogger.Log($"Prevented ownership transfer for NetworkEntity at id {entity.Id} due to failed validation!");
#endif

            return;
        }

        if (entity.IsOwnerLocked)
        {
            FusionLogger.Warn($"Attempted to transfer ownership for NetworkEntity at id {entity.Id}, but ownership was locked!");
            return;
        }

        if (!entity.IsRegistered)
        {
            FusionLogger.Warn($"Attempted to transfer ownership for NetworkEntity at id {entity.Id}, but it wasn't registered!");
            return;
        }

        var request = new EntityPlayerData()
        {
            PlayerId = ownerId.SmallId,
            Entity = new(entity),
        };

        MessageRelay.RelayNative(request, NativeMessageTag.EntityOwnershipRequest, NetworkChannel.Reliable, RelayType.ToServer);
    }

    public static void TakeOwnership(NetworkEntity entity)
    {
        // Don't allow taking ownership while interaction is disabled
        if (LocalControls.DisableInteraction)
        {
            return;
        }

        TransferOwnership(entity, PlayerIdManager.LocalId);
    }

    public static void HookEntityRegistered(ushort id, NetworkEntityDelegate callback)
    {
        var entity = IdManager.RegisteredEntities.GetEntity(id);

        if (entity != null)
        {
            callback(entity);
        }
        else
        {
            if (!_entityRegisteredCallbacks.ContainsKey(id))
            {
                _entityRegisteredCallbacks[id] = new();
            }

            _entityRegisteredCallbacks[id].Add(callback);
        }
    }
}