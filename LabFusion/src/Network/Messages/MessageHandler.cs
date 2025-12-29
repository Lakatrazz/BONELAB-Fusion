using LabFusion.Exceptions;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Network;

public abstract class MessageHandler
{
    public virtual ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.Both;

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
    /// Throws exceptions if the conditions set by <see cref="ExpectedReceiver"/> fail.
    /// </summary>
    /// <param name="received"></param>
    /// <exception cref="MessageExpectedServerException"></exception>
    /// <exception cref="MessageExpectedClientException"></exception>
    public void CheckExpectedConditions(ReceivedMessage received)
    {
        bool isServerHandled = received.IsServerHandled;

        if (ExpectedReceiver == ExpectedReceiverType.ServerOnly && !isServerHandled)
        {
            throw new MessageExpectedServerException();
        }
        else if (ExpectedReceiver == ExpectedReceiverType.ClientsOnly && isServerHandled)
        {
            throw new MessageExpectedClientException();
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