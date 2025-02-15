using LabFusion.Data;

namespace LabFusion.Network;

public struct ReceivedMessage
{
    /// <summary>
    /// The type of relay that this message was sent through.
    /// </summary>
    public RelayType Type { get; set; }

    /// <summary>
    /// The channel this message was sent through.
    /// </summary>
    public NetworkChannel Channel { get; set; }

    /// <summary>
    /// The small id of the message sender. Only valid if <see cref="Type"/> is NOT <see cref="RelayType.None"/>.
    /// </summary>
    public byte? Sender { get; set; }

    /// <summary>
    /// The target receiver of this message. Only valid if <see cref="Type"/> IS <see cref="RelayType.ToTarget"/>.
    /// </summary>
    public byte? Target { get; set; }

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
    /// <typeparam name="TData"></typeparam>
    /// <returns>The read data.</returns>
    public readonly TData ReadData<TData>() where TData : IFusionSerializable, new()
    {
        using var reader = FusionReader.Create(Bytes);

        var data = reader.ReadFusionSerializable<TData>();

        return data;
    }
}
