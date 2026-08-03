using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Extensions;

using ClientPlatformID = LabFusion.Network.ClientPlatformID;

namespace LabFusion.Player;

public class PlayerID : INetSerializable, IEquatable<PlayerID>
{
    /// <summary>
    /// Invoked when any PlayerID's metadata changes. Passes in the PlayerID, key, and value.
    /// </summary>
    public static event Action<PlayerID, string, string> OnMetadataChangedEvent, OnMetadataRemovedEvent;

    public bool IsMe => PlatformID == PlayerIDManager.LocalPlatformID;
    public bool IsValid => _isValid;
    private bool _isValid = false;

    public bool IsHost => SmallID == PlayerIDManager.HostSmallID;

    public ClientPlatformID PlatformID { get; private set; }
    public byte SmallID { get; private set; }

    private readonly PlayerMetadata _metadata = new();

    /// <summary>
    /// This Player's metadata. Only use this for getting metadata. To set your metadata, use <see cref="LocalPlayer.Metadata"/>.
    /// </summary>
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

    public PlayerID() 
    {
        _isValid = false;
    }

    public PlayerID(ClientPlatformID platformID, byte smallID, Dictionary<string, string> metadata)
    {
        Metadata.CreateMetadata();

        PlatformID = platformID;
        SmallID = smallID;

        foreach (var pair in metadata)
        {
            Metadata.Metadata.ForceSetLocalMetadata(pair.Key, pair.Value);
        }

        OnAfterCreateID();
    }

    private void OnAfterCreateID()
    {
        HookMetadata();

        _isValid = true;
    }

    private void HookMetadata()
    {
        Metadata.Metadata.OnTrySetMetadata += OnTrySetMetadata;
        Metadata.Metadata.OnTryRemoveMetadata += OnTryRemoveMetadata;

        Metadata.Metadata.OnMetadataChanged += OnMetadataChanged;
        Metadata.Metadata.OnMetadataRemoved += OnMetadataRemoved;
    }

    private void UnhookMetadata()
    {
        Metadata.DestroyMetadata();
    }

    private void OnMetadataChanged(string key, string value)
    {
        OnMetadataChangedEvent?.InvokeSafe(this, key, value, "executing OnMetadataChangedEvent");
    }

    private void OnMetadataRemoved(string key, string value)
    {
        OnMetadataRemovedEvent?.InvokeSafe(this, key, value, "executing OnMetadataRemovedEvent");
    }

    private bool OnTrySetMetadata(string key, string value)
    {
        if (!HasMetadataPermissions())
        {
            return false;
        }

        PlayerSender.SendPlayerMetadataRequest(SmallID, key, value);
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

    public bool Equals(PlayerID other)
    {
        if (other == null)
        {
            return false;
        }

        return SmallID == other.SmallID;
    }

    public override bool Equals(object obj)
    {
        if (obj is not PlayerID other)
        {
            return false;
        }

        return Equals(other);
    }

    public override int GetHashCode()
    {
        return SmallID.GetHashCode();
    }

    public static implicit operator byte(PlayerID id) => id.SmallID;

    public static bool IsNullOrInvalid(PlayerID id)
    {
        return id == null || !id.IsValid;
    }

    public void Insert() => PlayerIDManager.InsertPlayerID(this);

    public void Cleanup()
    {
        if (!_isValid)
        {
            FusionLogger.Warn("Attempted to cleanup a PlayerId that was not valid!");
            return;
        }

        PlayerIDManager.RemovePlayerID(this);

        if (PlayerIDManager.LocalID == this)
        {
            PlayerIDManager.RemoveLocalID();
        }

        _isValid = false;

        _onDestroyedEvent?.Invoke();
        _onDestroyedEvent = null;

        UnhookMetadata();
    }

    public int? GetSize() => sizeof(ulong) + sizeof(byte) + Metadata.Metadata.LocalDictionary.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        if (serializer.IsReader)
        {
            Metadata.CreateMetadata();
        }

        var platformID = PlatformID;
        var smallID = SmallID;
        var metadata = Metadata.Metadata.LocalDictionary;

        serializer.SerializeValue(ref platformID);
        serializer.SerializeValue(ref smallID);

        serializer.SerializeValue(ref metadata);

        if (serializer.IsReader)
        {
            PlatformID = platformID;
            SmallID = smallID;

            foreach (var pair in metadata)
            {
                Metadata.Metadata.ForceSetLocalMetadata(pair.Key, pair.Value);
            }

            OnAfterCreateID();
        }
    }
}