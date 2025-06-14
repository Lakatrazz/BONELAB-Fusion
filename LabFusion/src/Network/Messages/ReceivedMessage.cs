using LabFusion.Network.Serialization;

namespace LabFusion.Network;

/// <summary>
/// All of the information about a received message that is being handled.
/// </summary>
public struct ReceivedMessage
{
    /// <summary>
    /// The route that this message was sent through, including its relay type, network channel, and targets.
    /// </summary>
    public MessageRoute Route { get; set; }

    /// <summary>
    /// The small id of the message sender. Only valid if the <see cref="MessageRoute.Type"/> is NOT <see cref="RelayType.None"/>.
    /// </summary>
    public byte? Sender { get; set; }

    /// <summary>
    /// The bytes sent in this message.
    /// </summary>
    public byte[] Bytes { get; set; }

    /// <summary>
    /// Whether or not this message is being handled on the server's end. Not always true for the host, as it could be handled on the host's client.
    /// </summary>
    public bool IsServerHandled { get; set; }

    /// <summary>
    /// Reads the serializable that was written into this message.
    /// </summary>
    /// <typeparam name="TSerializable"></typeparam>
    /// <returns>The read data.</returns>
    public readonly TSerializable ReadData<TSerializable>() where TSerializable : INetSerializable, new()
    {
        using var reader = NetReader.Create(Bytes);

        TSerializable data = default;
        reader.SerializeValue(ref data);

        return data;
    }
}
