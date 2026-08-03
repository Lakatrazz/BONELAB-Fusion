using LabFusion.Network.Serialization;

using System.Text.Json.Serialization;

namespace LabFusion.Network;

/// <summary>
/// A unique identifier for a server that can be used to join it. This can change based on the platform and networking backend, so no specific data type or size should be expected unless this is being accessed within the platform's networking backend itself.
/// <para>Supports serialization in a Fusion message or through System.Text.Json.</para>
/// </summary>
[Serializable]
public struct ServerID : IEquatable<ServerID>, INetSerializable
{
    /// <summary>
    /// Represents an empty or invalid server ID.
    /// </summary>
    public static readonly ServerID Empty = new();

    [JsonPropertyName("value")]
    public string Value { readonly get => _value; set => _value = value; }

    private string _value;

    public ServerID(ulong id) { _value = id.ToString(); }

    public ServerID(string id) { _value = id; }

    public readonly bool IsValid() => !string.IsNullOrWhiteSpace(Value);

    public readonly int? GetSize() => Value.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _value);
    }

    public readonly override string ToString() => Value;

    public readonly bool Equals(ServerID other) => Value == other.Value;
    public readonly override bool Equals(object obj) => obj is ServerID id && Equals(id);

    public readonly override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ServerID left, ServerID right) => left.Equals(right);
    public static bool operator !=(ServerID left, ServerID right) => !(left == right);

    public static explicit operator ulong(ServerID serverID) => ulong.Parse(serverID.Value);
    public static explicit operator string(ServerID serverID) => serverID.Value;
}
