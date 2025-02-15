using LabFusion.Utilities;

namespace LabFusion.Network;

public abstract class MessageHandler
{
    public virtual ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.Both;

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