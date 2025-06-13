using LabFusion.Network.Serialization;

namespace LabFusion.Network;

/// <summary>
/// Necessary data written before the primary data of a message.
/// </summary>
public class MessagePrefix : INetSerializable
{
    public byte Tag;

    public MessageRoute Route;

    public byte? Sender = null;

    public int? GetSize()
    {
        return sizeof(byte) * 3 + Route.GetSize();
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Tag);
        serializer.SerializeValue(ref Route);

        if (Route.Type != RelayType.None)
        {
            serializer.SerializeValue(ref Sender);
        }
    }
}
