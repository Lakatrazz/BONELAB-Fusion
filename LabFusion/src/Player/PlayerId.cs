using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Metadata;
using LabFusion.SDK.Points;
using LabFusion.Senders;
using LabFusion.Utilities;

namespace LabFusion.Player;

public class PlayerId : INetSerializable, IEquatable<PlayerId>
{
    public bool IsMe => LongId == PlayerIdManager.LocalLongId;
    public bool IsValid => _isValid;
    private bool _isValid = false;

    public bool IsHost => SmallId == PlayerIdManager.HostSmallId;

    public ulong LongId { get; private set; }
    public byte SmallId { get; private set; }

    private readonly PlayerMetadata _metadata = new();
    public PlayerMetadata Metadata => _metadata;

    private Action _onDestroyedEvent = null;
    public event Action OnDestroyedEvent
    {
        add
        {
            if (!_isValid)
            {
                value();
            }
            else
            {
                _onDestroyedEvent += value;
            }
        }
        remove
        {
            _onDestroyedEvent -= value;
        }
    }

    private List<string> _internalEquippedItems = new List<string>();
    public List<string> EquippedItems => _internalEquippedItems;

    public PlayerId() 
    {
        _isValid = false;
    }

    public PlayerId(ulong longId, byte smallId, Dictionary<string, string> metadata, List<string> equippedItems)
    {
        Metadata.CreateMetadata();

        LongId = longId;
        SmallId = smallId;

        _internalEquippedItems = equippedItems;

        foreach (var pair in metadata)
        {
            Metadata.Metadata.ForceSetLocalMetadata(pair.Key, pair.Value);
        }

        OnAfterCreateId();
    }

    private void OnAfterCreateId()
    {
        HookMetadata();

        _isValid = true;
    }

    private void HookMetadata()
    {
        Metadata.Metadata.OnTrySetMetadata += OnTrySetMetadata;
        Metadata.Metadata.OnTryRemoveMetadata += OnTryRemoveMetadata;
    }

    private void UnhookMetadata()
    {
        Metadata.Metadata.OnTrySetMetadata -= OnTrySetMetadata;
        Metadata.Metadata.OnTryRemoveMetadata -= OnTryRemoveMetadata;

        Metadata.DestroyMetadata();
    }

    private bool OnTrySetMetadata(string key, string value)
    {
        if (!HasMetadataPermissions())
        {
            return false;
        }

        PlayerSender.SendPlayerMetadataRequest(SmallId, key, value);
        return true;
    }

    private bool OnTryRemoveMetadata(string key)
    {
        // Not implemented
        return false;
    }

    private bool HasMetadataPermissions()
    {
        return NetworkInfo.IsHost || IsMe;
    }

    public bool Equals(PlayerId other)
    {
        if (other == null)
        {
            return false;
        }

        return SmallId == other.SmallId;
    }

    public override bool Equals(object obj)
    {
        if (obj is not PlayerId other)
        {
            return false;
        }

        return Equals(other);
    }

    public override int GetHashCode()
    {
        return SmallId.GetHashCode();
    }

    public static implicit operator byte(PlayerId id) => id.SmallId;

    public static implicit operator ulong(PlayerId id) => id.LongId;

    public static bool IsNullOrInvalid(PlayerId id)
    {
        return id == null || !id.IsValid;
    }

    public bool HasEquipped(PointItem item)
    {
        if (IsMe)
            return item.IsEquipped;
        else
            return EquippedItems.Contains(item.Barcode);
    }

    internal void Internal_ForceSetEquipped(string barcode, bool value)
    {
        // Remove/add to the list
        if (value && !_internalEquippedItems.Contains(barcode))
        {
            _internalEquippedItems.Add(barcode);
        }
        else if (!value && _internalEquippedItems.Contains(barcode))
        {
            _internalEquippedItems.Remove(barcode);
        }

        // Invoke the events on the item
        PointItemManager.Internal_OnEquipChange(this, barcode, value);
    }

    public void Insert()
    {
        if (PlayerIdManager.PlayerIds.Any((id) => id.SmallId == SmallId))
        {
            var list = PlayerIdManager.PlayerIds.Where((id) => id.SmallId == SmallId).ToList();

            for (var i = 0; i < list.Count; i++)
            {
                list[i].Cleanup();
            }
        }

        PlayerIdManager.PlayerIds.Add(this);
    }

    public void Cleanup()
    {
        if (!_isValid)
        {
            FusionLogger.Warn("Attempted to cleanup a PlayerId that was not valid!");
            return;
        }

        PlayerIdManager.PlayerIds.Remove(this);

        if (PlayerIdManager.LocalId == this)
        {
            PlayerIdManager.RemoveLocalId();
        }

        _isValid = false;

        _onDestroyedEvent?.Invoke();
        _onDestroyedEvent = null;

        UnhookMetadata();
    }

    public void Serialize(INetSerializer serializer)
    {
        var longId = LongId;
        var smallId = SmallId;
        var metadata = Metadata.Metadata.LocalDictionary;
        var equippedItems = _internalEquippedItems.ToArray();

        serializer.SerializeValue(ref longId);
        serializer.SerializeValue(ref smallId);

        serializer.SerializeValue(ref metadata);
        serializer.SerializeValue(ref equippedItems);

        if (serializer.IsReader)
        {
            LongId = longId;
            SmallId = smallId;

            foreach (var pair in metadata)
            {
                Metadata.Metadata.ForceSetLocalMetadata(pair.Key, pair.Value);
            }

            foreach (var item in equippedItems)
            {
                Internal_ForceSetEquipped(item, true);
            }

            OnAfterCreateId();
        }
    }
}