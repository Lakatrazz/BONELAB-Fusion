using LabFusion.Data;
using LabFusion.Network;
using LabFusion.SDK.Metadata;
using LabFusion.SDK.Points;
using LabFusion.Senders;
using LabFusion.Utilities;

namespace LabFusion.Player;

public class PlayerId : IFusionSerializable, IEquatable<PlayerId>
{
    public bool IsMe => LongId == PlayerIdManager.LocalLongId;
    public bool IsValid => _isValid;
    private bool _isValid = false;

    public bool IsHost => SmallId == PlayerIdManager.LocalSmallId;

    public ulong LongId { get; private set; }
    public byte SmallId { get; private set; }

    private readonly NetworkMetadata _metadata = new();
    public NetworkMetadata Metadata => _metadata;

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

    public PlayerId(ulong longId, byte smallId, FusionDictionary<string, string> metadata, List<string> equippedItems)
    {
        LongId = longId;
        SmallId = smallId;

        _internalEquippedItems = equippedItems;

        foreach (var pair in metadata)
        {
            Metadata.ForceSetLocalMetadata(pair.Key, pair.Value);
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
        Metadata.OnTrySetMetadata += OnTrySetMetadata;
        Metadata.OnTryRemoveMetadata += OnTryRemoveMetadata;
    }

    private void UnhookMetadata()
    {
        Metadata.OnTrySetMetadata -= OnTrySetMetadata;
        Metadata.OnTryRemoveMetadata -= OnTryRemoveMetadata;
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
        return NetworkInfo.IsServer || IsMe;
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

    public void Serialize(FusionWriter writer)
    {
        writer.Write(LongId);
        writer.Write(SmallId);

        // Write the player metadata
        writer.Write(Metadata.LocalDictionary);

        writer.Write(_internalEquippedItems);
    }

    public void Deserialize(FusionReader reader)
    {
        LongId = reader.ReadUInt64();
        SmallId = reader.ReadByte();

        // Read the player metadata
        var metadata = reader.ReadStringDictionary();
        foreach (var pair in metadata)
        {
            Metadata.ForceSetLocalMetadata(pair.Key, pair.Value);
        }

        // Read equipped items
        var equippedItems = reader.ReadStrings();

        foreach (var item in equippedItems)
        {
            Internal_ForceSetEquipped(item, true);
        }

        OnAfterCreateId();
    }
}