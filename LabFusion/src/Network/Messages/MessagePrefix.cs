using LabFusion.Network.Serialization;

namespace LabFusion.Network;

/// <summary>
/// Necessary data written before the primary data of a message.
/// </summary>
public class MessagePrefix : INetSerializable
{
    /// <summary>
    /// The tag for the native message that's being sent.
    /// </summary>
    public byte Tag;

    /// <summary>
    /// The route that the message was sent on.
    /// </summary>
    public MessageRoute Route;

    /// <summary>
    /// The small ID of the message's sender.
    /// <para>If the small ID is null and it is being sent from the server to a client, 
    /// it indicates that the message was not relayed from any other client, but rather sent directly from the server.</para>
    /// <para>Otherwise, when a client sends a message to the server, it should typically be null as the server itself will replace it on relay.</para>
    /// </summary>
    public ClientSmallID? SenderSmallID = null;

    public int? GetSize() => sizeof(byte) + Route.GetSize() + SenderSmallID.GetNullableSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Tag);
        serializer.SerializeValue(ref Route);
        serializer.SerializeValue(ref SenderSmallID);
    }
}
