using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Entities;

public delegate void NetworkEntityDelegate(NetworkEntity entity);

public delegate void NetworkEntityPlayerDelegate(NetworkEntity entity, PlayerID player);

public class NetworkEntity : INetworkRegistrable, INetworkOwnable
{
    private ushort _id = 0;
    private ushort _queueID = 0;

    private bool _isRegistered = false;
    private bool _isQueued = false;
    private bool _isDestroyed = false;

    private bool _isOwnerLocked = false;

    public ushort ID => _id;

    private PlayerID _ownerID = null;

    public ushort QueueID => _queueID;

    public bool IsRegistered => _isRegistered;

    public bool IsQueued => _isQueued;

    public bool IsDestroyed => _isDestroyed;

    public PlayerID OwnerID => _ownerID;

    public bool IsOwner => HasOwner && _ownerID.IsMe;

    public bool HasOwner => _ownerID != null;

    public bool IsOwnerLocked => _isOwnerLocked;

    /// <summary>
    /// Invoked when the entity is unregistered.
    /// </summary>
    public event NetworkEntityDelegate OnEntityUnregistered;

    /// <summary>
    /// Invoked when a Player becomes the entity's new sync owner.
    /// </summary>
    public NetworkEntityPlayerDelegate OnEntityOwnershipTransfer;

    /// <summary>
    /// Invoked when a new Player joins the server and the creation of this NetworkEntity needs to be caught up.
    /// </summary>
    public event NetworkEntityPlayerDelegate OnEntityCreationCatchup;

    /// <summary>
    /// Invoked on the entity owner's end when a Player finishes creating this NetworkEntity and needs its data to be caught up.
    /// </summary>
    public event NetworkEntityPlayerDelegate OnEntityDataCatchup;

    private NetworkEntityDelegate _registeredCallback = null;

    private readonly List<byte> _dataCaughtUpPlayers = new();
    private readonly Dictionary<byte, NetworkEntityPlayerDelegate> _dataCatchupCallbacks = new();

    private readonly HashSet<IEntityExtender> _extenders = new();

    public void ConnectExtender(IEntityExtender extender)
    {
        _extenders.Add(extender);
    }

    public void DisconnectExtender(IEntityExtender extender)
    {
        _extenders.Remove(extender);
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
        byte smallID = playerID.SmallID;

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

        if (OnEntityCreationCatchup != null)
        {
            OnEntityCreationCatchup?.InvokeSafe(this, playerID, "executing OnEntityCreationCatchup");
            caughtUp = true;
        }

        return caughtUp;
    }

    internal bool InvokeDataCatchup(PlayerID playerID)
    {
        bool caughtUp = false;

        byte smallID = playerID.SmallID;

        if (OnEntityDataCatchup != null)
        {
            OnEntityDataCatchup?.InvokeSafe(this, playerID, "executing OnEntityDataCatchup");
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
            byte smallID = playerID.SmallID;

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
    public bool HasDataCaughtUp(PlayerID playerID) => _dataCaughtUpPlayers.Contains(playerID);

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
    }

    public void Unregister()
    {
        _isQueued = false;
        _queueID = 0;

        _isRegistered = false;
        _id = 0;

        _isDestroyed = true;

        OnEntityUnregistered?.Invoke(this);

        OnEntityUnregistered = null;
        OnEntityCreationCatchup = null;
        OnEntityDataCatchup = null;

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

        _ownerID = ownerID;

        OnEntityOwnershipTransfer?.Invoke(this, ownerID);
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

        _ownerID = null;

        OnEntityOwnershipTransfer?.Invoke(this, null);
    }

    public void LockOwner()
    {
        _isOwnerLocked = true;
    }

    public void UnlockOwner()
    {
        _isOwnerLocked = false;
    }
}
