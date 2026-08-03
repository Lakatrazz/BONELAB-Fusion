using LabFusion.Exceptions;
using LabFusion.Utilities;

namespace LabFusion.Network;

public abstract class MessageHandler
{
    public virtual ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.Both;

    /// <summary>
    /// Allows this message to be sent by Clients using the <see cref="RelayType.None"/> relay type.
    /// This should only be enabled for initial connection messages and nothing else for security reasons.
    /// <para>Regardless of this setting, the Server can always send messages to Clients using <see cref="RelayType.None"/>.</para>
    /// <para>Defaults to false.</para>
    /// </summary>
    public virtual bool AllowDirectRelay => false;

    public Net.NetAttribute[] NetAttributes { get; set; }

    internal virtual void StartHandlingMessage(ReceivedMessage received)
    {
        // If there are no attributes, just handle the message
        if (NetAttributes.Length <= 0)
        {
            FinishHandlingMessage(received);
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
            awaitable.HookComplete(() => { FinishHandlingMessage(received); });
        }
        else
        {
            FinishHandlingMessage(received);
        }
    }

    internal virtual void FinishHandlingMessage(ReceivedMessage received)
    {
        try
        {
            // Now handle the message info
            Handle(received);
        }
        catch (Exception e)
        {
            FusionLogger.LogException($"handling message of type {GetType().Name}", e);
        }
    }

    internal bool ProcessPreRelayMessage(ReceivedMessage received) => OnPreRelayMessage(received);

    /// <summary>
    /// Throws exceptions and/or disconnects the sender if the conditions set by <see cref="ExpectedReceiver"/> or <see cref="AllowDirectRelay"/> fail.
    /// </summary>
    /// <param name="received"></param>
    /// <exception cref="MessageExpectedServerException"></exception>
    /// <exception cref="MessageExpectedClientException"></exception>
    public void CheckExpectedConditions(ReceivedMessage received)
    {
        bool isServerHandled = received.IsServerHandled;

        // Check for the relay type
        bool isDirectRelay = received.Route.Type == RelayType.None;

        if (isServerHandled && !AllowDirectRelay && isDirectRelay)
        {
            DisconnectSenderAndThrowException();
            return;
        }

        // Check for the expected receiver
        if (ExpectedReceiver == ExpectedReceiverType.ServerOnly && !isServerHandled)
        {
            throw new MessageExpectedServerException();
        }
        else if (ExpectedReceiver == ExpectedReceiverType.ClientsOnly && isServerHandled)
        {
            throw new MessageExpectedClientException();
        }

        void DisconnectSenderAndThrowException()
        {
            var platformID = received.SenderPlatformID;

            if (platformID.HasValue)
            {
                NetworkConnectionManager.DisconnectUser(platformID.Value);
            }

            throw new MessageSpoofedException(platformID.ToString());
        }
    }

    public abstract void Handle(ReceivedMessage received);

    /// <summary>
    /// Invoked on the server's end before a message is relayed. Return true if the message is valid and can be relayed.
    /// </summary>
    /// <param name="received"></param>
    /// <returns></returns>
    protected virtual bool OnPreRelayMessage(ReceivedMessage received) => true;

    /// <summary>
    /// Invoked when the recipient has received the message and the message is ready to trigger its logic.
    /// </summary>
    /// <param name="received"></param>
    protected virtual void OnHandleMessage(ReceivedMessage received) { }
}