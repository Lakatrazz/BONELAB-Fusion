using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.Utilities;

using MelonLoader;

using System.Collections;

namespace LabFusion.Entities;

/// <summary>
/// Manages the registration and updating of network entities.
/// </summary>
public static class NetworkEntityManager
{
    /// <summary>
    /// The manager for network entity IDs.
    /// </summary>
    public static EntityIDManager<NetworkEntity> IDManager { get; } = new();

    /// <summary>
    /// The manager for network entity update loops.
    /// </summary>
    public static EntityUpdatableManager UpdatableManager { get; } = new();

    /// <summary>
    /// The list of validators that can determine if a network entity is able to transfer ownership.
    /// </summary>
    public static EntityValidationList OwnershipTransferValidators { get; } = new();

    private static readonly Dictionary<NetworkEntityID, List<NetworkEntityDelegate>> _entityRegisteredCallbacks = new();

    /// <summary>
    /// Gets and returns a NetworkEntity from its id, or null if none is found.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static NetworkEntity GetEntity(NetworkEntityID id) => IDManager.RegisteredEntities.GetEntity(id);

    /// <summary>
    /// Tries to get a NetworkEntity from its ID and returns if it was found.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static bool TryGetEntity(NetworkEntityID id, out NetworkEntity entity)
    {
        entity = GetEntity(id);
        return entity != null;
    }

    /// <summary>
    /// Requests an ownership transfer for a network entity to a given player.
    /// <para>This will likely be replaced in the future by server side ownership transfer for safety, so try not to rely on it.</para>
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="ownerID"></param>
    public static void TransferOwnership(NetworkEntity entity, PlayerID ownerID)
    {
        if (!OwnershipTransferValidators.Validate(entity, ownerID))
        {
#if DEBUG
            FusionLogger.Log($"Prevented ownership transfer for NetworkEntity at ID {entity.ID} due to failed validation!");
#endif

            return;
        }

        if (entity.IsOwnerLocked)
        {
            FusionLogger.Warn($"Attempted to transfer ownership for NetworkEntity at ID {entity.ID}, but ownership was locked!");
            return;
        }

        if (!entity.IsRegistered)
        {
            FusionLogger.Warn($"Attempted to transfer ownership for NetworkEntity at ID {entity.ID}, but it wasn't registered!");
            return;
        }

        var request = new EntityPlayerData()
        {
            PlayerID = ownerID.SmallID,
            Entity = new(entity),
        };

        ClientManager.RelayNative(request, NativeMessageTag.EntityOwnershipRequest, CommonMessageRoutes.ReliableToServer);
    }

    /// <summary>
    /// Takes ownership of a network entity.
    /// <para>This will likely be replaced in the future by server side ownership transfer for safety, so try not to rely on it.</para>
    /// </summary>
    /// <param name="entity"></param>
    public static void TakeOwnership(NetworkEntity entity)
    {
        // Don't allow taking ownership while interaction is disabled
        if (LocalControls.DisableInteraction)
        {
            return;
        }

        TransferOwnership(entity, PlayerIDManager.LocalID);
    }

    /// <summary>
    /// Hooks a callback to run when a network entity is registered for a certain ID.
    /// <para>It is recommended to avoid this unless absolutely necessary, as the callback can potentially invoke for entities that reuse IDs.</para>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="callback"></param>
    public static void HookEntityRegistered(NetworkEntityID id, NetworkEntityDelegate callback)
    {
        var entity = GetEntity(id);

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

    /// <summary>
    /// Resets the incrementing server IDs for network entities to be the lowest unused ID.
    /// This should be called when a large amount of entities are known to be unregistered, like when changing levels.
    /// </summary>
    public static void CleanupIDs()
    {
        IDManager.RegisteredEntities.ClearID();

        _entityRegisteredCallbacks.Clear();
    }

    /// <summary>
    /// Unregisters all network entities. This should be called when the client disconnects or the server is closed.
    /// </summary>
    public static void CleanupEntities()
    {
        // Clear registered entities
        var registeredEntities = IDManager.RegisteredEntities.EntityByID.Keys.ToList();

        foreach (var entity in registeredEntities)
        {
            try
            {
                IDManager.UnregisterEntity(entity);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("unregistering NetworkEntity", e);
            }
        }

        IDManager.RegisteredEntities.Clear();

        _entityRegisteredCallbacks.Clear();
    }

    internal static void Initialize()
    {
        CatchupManager.PlayerServerCatchingUp += OnPlayerServerCatchup;
        IDManager.EntityRegistered += OnEntityRegistered;
        PlayerIDManager.PlayerLeft += OnPlayerLeft;
    }

    internal static void OnUpdate(float deltaTime) => UpdatableManager.OnEntityUpdate(deltaTime);

    internal static void OnFixedUpdate(float deltaTime) => UpdatableManager.OnEntityFixedUpdate(deltaTime);

    internal static void OnLateUpdate(float deltaTime) => UpdatableManager.OnEntityLateUpdate(deltaTime);

    private static void OnEntityRegistered(NetworkEntity entity)
    {
        var id = entity.ID;

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

    private static void OnPlayerLeft(PlayerID playerID)
    {
        foreach (var entity in IDManager.RegisteredEntities.IDByEntity.Values)
        {
            entity.OnPlayerLeft(playerID);
        }
    }

    private static void OnPlayerServerCatchup(PlayerID playerID)
    {
        MelonCoroutines.Start(SendCreationCatchupCoroutine(playerID));
    }

    private static IEnumerator SendCreationCatchupCoroutine(PlayerID playerID)
    {
        var catchupQueue = new Queue<NetworkEntity>(IDManager.RegisteredEntities.IDByEntity.Values);

        while (catchupQueue.Count > 0 && !FusionSceneManager.IsLoading() && playerID.IsValid)
        {
            var entity = catchupQueue.Dequeue();

            if (!entity.IsRegistered)
            {
                continue;
            }

            bool sent = SendCreationCatchup(entity, playerID);

            if (!sent)
            {
                continue;
            }

            yield return null;

            yield return null;
        }
    }

    private static bool SendCreationCatchup(NetworkEntity entity, PlayerID playerId)
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
}