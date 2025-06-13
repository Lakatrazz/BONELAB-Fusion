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

        var type = Route.Type;
        var channel = Route.Channel;
        byte? target = Route.Target;
        ArraySegment<byte> targets = Route.Targets;

        serializer.SerializeValue(ref type, Precision.OneByte);
        serializer.SerializeValue(ref channel, Precision.OneByte);

        if (type == RelayType.ToTarget)
        {
            serializer.SerializeValue(ref target);
        }

        if (type == RelayType.ToTargets)
        {
            serializer.SerializeValue(ref targets);
        }

        if (type != RelayType.None)
        {
            serializer.SerializeValue(ref Sender);
        }

        if (serializer.IsReader)
        {
            Route = new MessageRoute()
            {
                Type = type,
                Channel = channel,
                Target = target,
                Targets = targets,
            };
        }
    }
}
