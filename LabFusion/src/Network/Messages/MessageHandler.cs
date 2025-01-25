using LabFusion.Data;
using LabFusion.Utilities;

namespace LabFusion.Network;

public enum ExpectedType
{
    /// <summary>
    /// This message is expected to be received on both clients and the server.
    /// </summary>
    Both,

    /// <summary>
    /// This message should only ever be received on the server.
    /// </summary>
    ServerOnly,

    /// <summary>
    /// This message should only ever be received on the clients.
    /// </summary>
    ClientsOnly,
}

public enum RelayType : byte
{
    /// <summary>
    /// Relays the message to the server, but without a proper "Sender" set. Only use this before a proper ID has been established.
    /// </summary>
    None,

    /// <summary>
    /// Relays the message to only the server.
    /// </summary>
    ToServer,

    /// <summary>
    /// Relays the message to all clients including the sender.
    /// </summary>
    ToClients,

    /// <summary>
    /// Relays the message to all other clients except for the sender.
    /// </summary>
    ToOtherClients,

    /// <summary>
    /// Relays the message to a set target user.
    /// </summary>
    ToTarget,
}

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

public abstract class MessageHandler
{
    public virtual ExpectedType ExpectedReceiver => ExpectedType.Both;

    public Net.NetAttribute[] NetAttributes { get; set; }

    protected virtual void Internal_HandleMessage(ReceivedMessage received)
    {
        // If there are no attributes, just handle the message
        if (NetAttributes.Length <= 0)
        {
            Internal_FinishMessage(received);
            return;
        }

        // Initialize the attribute info
        for (var i = 0; i < NetAttributes.Length; i++)
        {
            var attribute = NetAttributes[i];
            attribute.OnHandleBegin();
        }

        // Check if we should already stop handling
        for (var i = 0; i < NetAttributes.Length; i++)
        {
            var attribute = NetAttributes[i];

            if (attribute.StopHandling())
                return;
        }

        // Check for any awaitable attributes
        Net.NetAttribute awaitable = null;

        for (var i = 0; i < NetAttributes.Length; i++)
        {
            var attribute = NetAttributes[i];

            if (attribute.IsAwaitable())
            {
                awaitable = attribute;
                break;
            }
        }

        // Hook the awaitable attribute so that we can handle the message when its ready
        if (awaitable != null)
        {
            awaitable.HookComplete(() => { Internal_FinishMessage(received); });
        }
        else
        {
            Internal_FinishMessage(received);
        }
    }

    protected virtual void Internal_FinishMessage(ReceivedMessage received)
    {
        try
        {
            // Now handle the message info
            Handle(received);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("handling message", e);
        }
    }

    [Obsolete("Please override OnHandleMessage instead.")]
    public virtual void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
    }

    public abstract void Handle(ReceivedMessage received);

    protected virtual void OnHandleMessage(ReceivedMessage received) { }
}