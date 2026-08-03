using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Entities;

public delegate void NetworkEntityDelegate(NetworkEntity entity);

public delegate void NetworkEntityPlayerDelegate(NetworkEntity entity, PlayerID player);

/// <summary>
/// An entity that is synchronized on the network with a given ID.
/// </summary>
public sealed class NetworkEntity : INetworkRegistrable, INetworkOwnable
{
    private ushort _id = 0;
    private ushort _queueID = 0;

    private bool _isRegistered = false;
    private bool _isQueued = false;
    private bool _isDestroyed = false;

    private bool _isOwnerLocked = false;

    private PlayerID _ownerID = null;

    /// <summary>
    /// The identifier for this entity.
    /// </summary>
    public ushort ID => _id;

    /// <summary>
    /// If the entity has not been registered yet and is waiting for an ID from the server, a queue ID is given.
    /// </summary>
    public ushort QueueID => _queueID;

    /// <summary>
    /// Whether or not this entity is registered and ready.
    /// </summary>
    public bool IsRegistered => _isRegistered;

    /// <summary>
    /// Whether or not this entity is waiting for an ID from the server.
    /// </summary>
    public bool IsQueued => _isQueued;

    /// <summary>
    /// Whether or not this entity has been destroyed and is no longer valid.
    /// </summary>
    public bool IsDestroyed => _isDestroyed;

    /// <summary>
    /// The owner of this entity that manages simulation.
    /// </summary>
    public PlayerID OwnerID => _ownerID;

    /// <summary>
    /// Whether or not the local player owns this entity.
    /// </summary>
    public bool IsOwner => HasOwner && OwnerID.IsMe;

    /// <summary>
    /// Whether or not this entity has a valid owner.
    /// </summary>
    public bool HasOwner => OwnerID != null;

    /// <summary>
    /// Whether or not the owner of this entity can be changed.
    /// </summary>
    public bool IsOwnerLocked => _isOwnerLocked;

    /// <summary>
    /// The source of the entity's creation.
    /// </summary>
    public EntitySource Source { get; set; } = EntitySource.None;

    /// <summary>
    /// Entities that are directly linked to this one. 
    /// Entities can become linked through specific interactions with each other, such as being welded to one another.
    /// This allows for ownership to be propagated through linked entities to maintain stability.
    /// </summary>
    public IReadOnlyList<NetworkEntity> LinkedEntities => _linkedEntities;

    /// <summary>
    /// Returns whether any entities have been directly linked to this entity.
    /// </summary>
    public bool HasLinkedEntities => LinkedEntities.Count > 0;

    /// <summary>
    /// Invoked when the entity is unregistered.
    /// </summary>
    public event NetworkEntityDelegate EntityUnregistered;

    /// <summary>
    /// Invoked when a Player becomes the entity's new sync owner.
    /// </summary>
    public NetworkEntityPlayerDelegate EntityOwnershipTransferred;

    /// <summary>
    /// Invoked when a new Player joins the server and the creation of this NetworkEntity needs to be caught up.
    /// </summary>
    public event NetworkEntityPlayerDelegate EntityCreationCatchingUp;

    /// <summary>
    /// Invoked on the entity owner's end when a Player finishes creating this NetworkEntity and needs its data to be caught up.
    /// </summary>
    public event NetworkEntityPlayerDelegate EntityDataCatchingUp;

    private NetworkEntityDelegate _registeredCallback = null;

    private readonly List<ClientSmallID> _dataCaughtUpPlayers = new();
    private readonly Dictionary<ClientSmallID, NetworkEntityPlayerDelegate> _dataCatchupCallbacks = new();

    private readonly HashSet<IEntityExtender> _extenders = new();
    private bool _isUnregisteringExtenders = false;

    private readonly List<NetworkEntity> _linkedEntities = new();

    public void LinkEntity(NetworkEntity entity)
    {
        LinkEntityOneWay(entity);
        entity.LinkEntityOneWay(this);
    }

    public void UnlinkEntity(NetworkEntity entity)
    {
        UnlinkEntityOneWay(entity);
        entity.UnlinkEntityOneWay(this);
    }

    public void UnlinkEntities()
    {
        foreach (var entity in LinkedEntities)
        {
            entity.UnlinkEntityOneWay(this);
        }

        UnlinkEntitiesOneWay();
    }

    private void LinkEntityOneWay(NetworkEntity entity)
    {
        if (_linkedEntities.Contains(entity))
        {
            return;
        }

        _linkedEntities.Add(entity);
    }

    private void UnlinkEntityOneWay(NetworkEntity entity)
    {
        _linkedEntities.Remove(entity);
    }

    private void UnlinkEntitiesOneWay()
    {
        _linkedEntities.Clear();
    }

    /// <summary>
    /// Connects an extender to this NetworkEntity. 
    /// If the NetworkEntity is already registered, it will also immediately invoke <see cref="IEntityExtender.OnExtenderRegistered"/>.
    /// Otherwise, the method will be invoked when the NetworkEntity gets registered.
    /// </summary>
    /// <param name="extender"></param>
    public void ConnectExtender(IEntityExtender extender)
    {
        bool added = _extenders.Add(extender);

        if (!added)
        {
            return;
        }

        if (IsRegistered)
        {
            RegisterExtender(extender);
        }
    }

    /// <summary>
    /// Disconnects an extender from this NetworkEntity.
    /// If the NetworkEntity is registered, it will immediately invoke <see cref="IEntityExtender.OnExtenderUnregistered"/>.
    /// Otherwise, it is assumed that the method was already invoked when the NetworkEntity was unregistered before.
    /// </summary>
    /// <param name="extender"></param>
    public void DisconnectExtender(IEntityExtender extender)
    {
        if (_isUnregisteringExtenders)
        {
            return;
        }

        bool removed = _extenders.Remove(extender);

        if (!removed)
        {
            return;
        }

        if (IsRegistered)
        {
            UnregisterExtender(extender);
        }
    }

    public TExtender GetExtender<TExtender>() where TExtender : IEntityExtender
    {
        foreach (var extender in _extenders)
        {
            if (extender is TExtender result)
            {
                return result;
            }
        }

        return default;
    }

    public IEntityExtender GetExtender(Type type)
    {
        foreach (var extender in _extenders)
        {
            if (type.IsAssignableFrom(extender.GetType()))
            {
                return extender;
            }
        }

        return null;
    }

    internal void OnPlayerLeft(PlayerID playerID)
    {
        ClientSmallID smallID = playerID.SmallID;

        _dataCaughtUpPlayers.Remove(smallID);
        _dataCatchupCallbacks.Remove(smallID);

        if (OwnerID == playerID)
        {
            RemoveOwner();
        }
    }

    internal bool InvokeCreationCatchup(PlayerID playerID)
    {
        bool caughtUp = false;

        if (EntityCreationCatchingUp != null)
        {
            EntityCreationCatchingUp?.InvokeSafe(this, playerID, "executing OnEntityCreationCatchup");
            caughtUp = true;
        }

        return caughtUp;
    }

    internal bool InvokeDataCatchup(PlayerID playerID)
    {
        bool caughtUp = false;

        ClientSmallID smallID = playerID.SmallID;

        if (EntityDataCatchingUp != null)
        {
            EntityDataCatchingUp?.InvokeSafe(this, playerID, "executing OnEntityDataCatchup");
            caughtUp = true;
        }

        if (_dataCatchupCallbacks.TryGetValue(smallID, out var callback))
        {
            _dataCatchupCallbacks.Remove(smallID);

            callback?.InvokeSafe(this, playerID, "executing data catchup callback");

            caughtUp = true;
        }

        if (!_dataCaughtUpPlayers.Contains(smallID))
        {
            _dataCaughtUpPlayers.Add(smallID);
        }

        return caughtUp;
    }

    /// <summary>
    /// Registers a callback for when the NetworkEntity is registered. If the entity is already registered, this will invoke immediately.
    /// </summary>
    /// <param name="registeredCallback"></param>
    public void HookOnRegistered(NetworkEntityDelegate registeredCallback)
    {
        if (IsRegistered)
        {
            registeredCallback?.Invoke(this);
        }
        else
        {
            _registeredCallback += registeredCallback;
        }
    }

    /// <summary>
    /// Registers a callback for when a Player requests data catchup for a NetworkEntity. If they've already requested it, the callback invokes immediately.
    /// <para>Hook into this when catchup depends on multiple NetworkEntities.</para>
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="dataCatchupCallback"></param>
    public void HookOnDataCatchup(PlayerID playerID, NetworkEntityPlayerDelegate dataCatchupCallback)
    {
        if (HasDataCaughtUp(playerID))
        {
            dataCatchupCallback?.Invoke(this, playerID);
        }
        else
        {
            ClientSmallID smallID = playerID.SmallID;

            if (!_dataCatchupCallbacks.ContainsKey(smallID))
            {
                _dataCatchupCallbacks[smallID] = null;
            }

            _dataCatchupCallbacks[smallID] += dataCatchupCallback;
        }
    }

    /// <summary>
    /// Returns if this NetworkEntity has already had data catchup requested from a specific Player.
    /// </summary>
    /// <param name="playerID"></param>
    /// <returns></returns>
    public bool HasDataCaughtUp(PlayerID playerID) => _dataCaughtUpPlayers.Contains(playerID.SmallID);

    /// <summary>
    /// Clears the players that have had data catch up for this NetworkEntity.
    /// </summary>
    public void ClearDataCaughtUpPlayers()
    {
        _dataCaughtUpPlayers.Clear();
    }

    public void Queue(ushort queuedId)
    {
        _queueID = queuedId;
        _isQueued = true;

        _isRegistered = false;
        _id = 0;
    }

    public void Register(ushort id)
    {
        _isQueued = false;
        _queueID = 0;

        _isRegistered = true;
        _id = id;

        _registeredCallback?.Invoke(this);
        _registeredCallback = null;

        RegisterExtenders();
    }

    public void Unregister()
    {
        _isQueued = false;
        _queueID = 0;

        _isRegistered = false;
        _id = 0;

        _isDestroyed = true;

        EntityUnregistered?.Invoke(this);

        UnregisterExtenders();
        DisconnectExtenders();

        EntityUnregistered = null;
        EntityCreationCatchingUp = null;
        EntityDataCatchingUp = null;

        UnlinkEntities();

        RemoveOwner();
    }

    public void SetOwner(PlayerID ownerID)
    {
        if (IsOwnerLocked)
        {
#if DEBUG
            FusionLogger.Warn($"Tried setting the owner of a NetworkEntity at id {ID} to {ownerID.SmallID}, but it was locked!");
#endif
            return;
        }

        if (OwnerID == ownerID)
        {
            return;
        }

        _ownerID = ownerID;

        EntityOwnershipTransferred?.Invoke(this, ownerID);
    }

    public void RemoveOwner()
    {
        if (IsOwnerLocked)
        {
#if DEBUG
            FusionLogger.Warn($"Tried removing the owner of a NetworkEntity at id {ID}, but it was locked!");
#endif
            return;
        }

        if (OwnerID == null)
        {
            return;
        }

        _ownerID = null;

        EntityOwnershipTransferred?.Invoke(this, null);
    }

    public void LockOwner()
    {
        _isOwnerLocked = true;
    }

    public void UnlockOwner()
    {
        _isOwnerLocked = false;
    }

    private void RegisterExtenders()
    {
        var snapshottedExtenders = _extenders.ToArray();

        foreach (var extender in snapshottedExtenders)
        {
            RegisterExtender(extender);
        }
    }

    private void UnregisterExtenders()
    {
        _isUnregisteringExtenders = true;

        foreach (var extender in _extenders)
        {
            UnregisterExtender(extender);
        }

        _isUnregisteringExtenders = false;
    }

    private void DisconnectExtenders()
    {
        _extenders.Clear();
    }

    private static void RegisterExtender(IEntityExtender extender)
    {
        try
        {
            extender.OnExtenderRegistered();
        }
        catch (Exception e)
        {
            FusionLogger.LogException("registering extender", e);
        }
    }

    private static void UnregisterExtender(IEntityExtender extender)
    {
        try
        {
            extender.OnExtenderUnregistered();
        }
        catch (Exception e)
        {
            FusionLogger.LogException("unregistering extender", e);
        }
    }
}
