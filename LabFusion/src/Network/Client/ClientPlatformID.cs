using LabFusion.Network.Serialization;

using System.Text.Json.Serialization;

namespace LabFusion.Network;

/// <summary>
/// A unique, persistent identifier for a client.
/// This can change based on the platform and networking backend, so no specific data type or size should be expected unless this is being accessed within the platform's networking backend itself.
/// <para>Supports serialization in a Fusion message or through System.Text.Json.</para>
/// </summary>
[Serializable]
public struct ClientPlatformID : IEquatable<ClientPlatformID>, INetSerializable
{
    /// <summary>
    /// Represents an empty or invalid platform ID.
    /// </summary>
    public static readonly ClientPlatformID Empty = new();

    [JsonPropertyName("value")]
    public string Value { readonly get => _value; set => _value = value; }

    private string _value;

    public ClientPlatformID(ulong id) { _value = id.ToString(); }

    public ClientPlatformID(string id) { _value = id; }

    public readonly bool IsValid() => !string.IsNullOrWhiteSpace(Value);

    public readonly int? GetSize() => Value.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _value);
    }

    public readonly override string ToString() => Value;

    public readonly bool Equals(ClientPlatformID other) => Value == other.Value;
    public readonly override bool Equals(object obj) => obj is ClientPlatformID id && Equals(id);

    public readonly override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ClientPlatformID left, ClientPlatformID right) => left.Equals(right);
    public static bool operator !=(ClientPlatformID left, ClientPlatformID right) => !(left == right);

    public static explicit operator ulong(ClientPlatformID platformID) => ulong.Parse(platformID.Value);
    public static explicit operator string(ClientPlatformID platformID) => platformID.Value;
}
