using LabFusion.Network.Serialization;

using System.Text.Json.Serialization;

namespace LabFusion.Entities;

/// <summary>
/// A session dependent unique identifier for a network entity.
/// The underlying data type is not guaranteed to stay the same in the future.
/// <para>Supports serialization in a Fusion message or through System.Text.Json.</para>
/// </summary>
[Serializable]
public struct NetworkEntityID : INetSerializable
{
    /// <summary>
    /// Represents an empty or invalid network entity ID.
    /// </summary>
    public static readonly NetworkEntityID Empty = new();

    /// <summary>
    /// The size of a network entity ID in bytes.
    /// </summary>
    public static readonly int Size = sizeof(ushort);

    /// <summary>
    /// The internal value of the NetworkEntityID. The data type is not guaranteed to stay the same in the future.
    /// </summary>
    [JsonPropertyName("value")]
    public ushort Value { readonly get => _value; set => _value = value; }

    private ushort _value;

    public NetworkEntityID(ushort id) { _value = id; }
    public NetworkEntityID(byte id) : this((ushort)id) { }
    public NetworkEntityID(int id) : this((ushort)id) { }

    public readonly int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _value);
    }

    /// <summary>
    /// Returns the network entity ID that would succeed this one.
    /// </summary>
    /// <returns></returns>
    public readonly NetworkEntityID GetNext() => new(Value + 1);

    /// <summary>
    /// Returns true if there is space for another network entity ID after this one.
    /// </summary>
    /// <returns></returns>
    public readonly bool HasSpace() => Value < ushort.MaxValue;

    /// <summary>
    /// Tries to parse a network entity ID from its string representation.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryParse(string value, out NetworkEntityID result)
    {
        if (ushort.TryParse(value, out var output))
        {
            result = new NetworkEntityID(output);
            return true;
        }

        result = Empty;
        return false;
    }

    public readonly override string ToString() => Value.ToString();

    public readonly bool Equals(NetworkEntityID other) => Value == other.Value;
    public readonly override bool Equals(object obj) => obj is NetworkEntityID id && Equals(id);

    public readonly override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(NetworkEntityID left, NetworkEntityID right) => left.Equals(right);
    public static bool operator !=(NetworkEntityID left, NetworkEntityID right) => !(left == right);

    public static bool operator <(NetworkEntityID left, NetworkEntityID right) => left.Value < right.Value;
    public static bool operator >(NetworkEntityID left, NetworkEntityID right) => left.Value > right.Value;
    public static bool operator <=(NetworkEntityID left, NetworkEntityID right) => left.Value <= right.Value;
    public static bool operator >=(NetworkEntityID left, NetworkEntityID right) => left.Value >= right.Value;

    public static explicit operator byte(NetworkEntityID entityID) => (byte)entityID.Value;
    public static explicit operator ushort(NetworkEntityID entityID) => entityID.Value;
    public static explicit operator int(NetworkEntityID entityID) => entityID.Value;
}
