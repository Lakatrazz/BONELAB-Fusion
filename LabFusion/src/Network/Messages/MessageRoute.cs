using LabFusion.Network.Serialization;

namespace LabFusion.Network;

/// <summary>
/// The route for a message, including its relay type, network channel, and target recipients.
/// </summary>
public struct MessageRoute : INetSerializable
{
    /// <summary>
    /// The type of relay that this message is sent through.
    /// </summary>
    public RelayType Type { get; set; }

    /// <summary>
    /// The channel this message is sent through.
    /// </summary>
    public NetworkChannel Channel { get; set; }

    /// <summary>
    /// The target receiver of this message. Only valid if <see cref="Type"/> is <see cref="RelayType.ToTarget"/>.
    /// </summary>
    public byte? Target { get; set; }

    /// <summary>
    /// Multiple target receivers of this message. Only valid if <see cref="Type"/> is <see cref="RelayType.ToTargets"/>.
    /// </summary>
    public ArraySegment<byte> Targets { get; set; }

    public MessageRoute(RelayType type, NetworkChannel channel)
    {
        Type = type;
        Channel = channel;
        Target = null;
        Targets = ArraySegment<byte>.Empty;
    }

    public MessageRoute(byte target, NetworkChannel channel)
    {
        Type = RelayType.ToTarget;
        Channel = channel;
        Target = target;
        Targets = ArraySegment<byte>.Empty;
    }

    public MessageRoute(ArraySegment<byte> targets, NetworkChannel channel)
    {
        Type = RelayType.ToTargets;
        Channel = channel;
        Target = null;
        Targets = targets;
    }

    public MessageRoute(NetworkChannel channel, params byte[] targets) : this(new ArraySegment<byte>(targets), channel) { }

    public readonly int GetSize()
    {
        int size = sizeof(byte) * 2;

        if (Type == RelayType.ToTarget)
        {
            size += sizeof(byte) * 2;
        }

        if (Type == RelayType.ToTargets)
        {
            size += sizeof(int) + sizeof(byte) * Targets.Count;
        }

        return size;
    }

    public void Serialize(INetSerializer serializer)
    {
        var type = Type;
        var channel = Channel;
        byte? target = Target;
        ArraySegment<byte> targets = Targets;

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

        if (serializer.IsReader)
        {
            Type = type;
            Channel = channel;
            Target = target;
            Targets = targets;
        }
    }
}
