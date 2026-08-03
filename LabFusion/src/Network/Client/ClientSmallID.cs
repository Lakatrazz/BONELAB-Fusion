using LabFusion.Network.Serialization;

using System.Text.Json.Serialization;

namespace LabFusion.Network;

/// <summary>
/// A session dependent unique identifier for a client in order to lessen the amount of data sent compared to a <see cref="ClientPlatformID"/>.
/// The underlying data type is not guaranteed to stay the same in the future.
/// <para>Supports serialization in a Fusion message or through System.Text.Json.</para>
/// </summary>
[Serializable]
public struct ClientSmallID : IEquatable<ClientSmallID>, INetSerializable
{
    /// <summary>
    /// Represents an empty or invalid small ID.
    /// </summary>
    public static readonly ClientSmallID Empty = new();

    [JsonPropertyName("value")]
    public byte Value { readonly get => _value; set => _value = value; }

    private byte _value;

    public ClientSmallID(byte id) { _value = id; }
    public ClientSmallID(ushort id) : this((byte)id) { }
    public ClientSmallID(int id) : this((byte)id) { }

    public readonly int? GetSize() => sizeof(byte);

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _value);
    }

    public static bool TryParse(string value, out ClientSmallID result)
    {
        if (byte.TryParse(value, out var output))
        {
            result = new ClientSmallID(output);
            return true;
        }

        result = Empty;
        return false;
    }

    public readonly override string ToString() => Value.ToString();

    public readonly bool Equals(ClientSmallID other) => Value == other.Value;
    public readonly override bool Equals(object obj) => obj is ClientSmallID id && Equals(id);

    public readonly override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ClientSmallID left, ClientSmallID right) => left.Equals(right);
    public static bool operator !=(ClientSmallID left, ClientSmallID right) => !(left == right);

    public static explicit operator byte(ClientSmallID smallID) => smallID.Value;
    public static explicit operator ushort(ClientSmallID smallID) => smallID.Value;
    public static explicit operator int(ClientSmallID smallID) => smallID.Value;
}
