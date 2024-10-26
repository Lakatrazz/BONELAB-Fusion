using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Entities;

public delegate void NetworkEntityDelegate(NetworkEntity entity);

public delegate void NetworkEntityPlayerDelegate(NetworkEntity entity, PlayerId player);

public class NetworkEntity : INetworkRegistrable, INetworkOwnable
{
    private ushort _id = 0;
    private ushort _queueId = 0;

    private bool _isRegistered = false;
    private bool _isQueued = false;
    private bool _isDestroyed = false;

    private bool _isOwnerLocked = false;

    public ushort Id => _id;

    private PlayerId _ownerId = null;

    public ushort QueueId => _queueId;

    public bool IsRegistered => _isRegistered;

    public bool IsQueued => _isQueued;

    public bool IsDestroyed => _isDestroyed;

    public PlayerId OwnerId => _ownerId;

    public bool IsOwner => HasOwner && _ownerId.IsMe;

    public bool HasOwner => _ownerId != null;

    public bool IsOwnerLocked => _isOwnerLocked;

    public event NetworkEntityDelegate OnEntityUnregistered;
    public event NetworkEntityPlayerDelegate OnEntityCatchup, OnEntityOwnershipTransfer;

    private NetworkEntityDelegate _registeredCallback = null;

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

    public void InvokeCatchup(PlayerId playerId)
    {
        OnEntityCatchup?.Invoke(this, playerId);
    }

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

    public void Queue(ushort queuedId)
    {
        _queueId = queuedId;
        _isQueued = true;

        _isRegistered = false;
        _id = 0;
    }

    public void Register(ushort id)
    {
        _isQueued = false;
        _queueId = 0;

        _isRegistered = true;
        _id = id;

        _registeredCallback?.Invoke(this);
        _registeredCallback = null;
    }

    public void Unregister()
    {
        _isQueued = false;
        _queueId = 0;

        _isRegistered = false;
        _id = 0;

        _isDestroyed = true;

        OnEntityUnregistered?.Invoke(this);

        OnEntityUnregistered = null;
        OnEntityCatchup = null;

        RemoveOwner();
    }

    public void SetOwner(PlayerId ownerId)
    {
        if (IsOwnerLocked)
        {
#if DEBUG
            FusionLogger.Warn($"Tried setting the owner of a NetworkEntity at id {Id} to {ownerId.SmallId}, but it was locked!");
#endif
            return;
        }

        _ownerId = ownerId;

        OnEntityOwnershipTransfer?.Invoke(this, ownerId);
    }

    public void RemoveOwner()
    {
        if (IsOwnerLocked)
        {
#if DEBUG
            FusionLogger.Warn($"Tried removing the owner of a NetworkEntity at id {Id}, but it was locked!");
#endif
            return;
        }

        _ownerId = null;

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
