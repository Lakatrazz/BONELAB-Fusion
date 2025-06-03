using LabFusion.Exceptions;
using LabFusion.Network;

namespace LabFusion.SDK.Modules;

public abstract class ModuleMessageHandler : MessageHandler
{
    internal long? _tag = null;
    public long? Tag => _tag;

    public sealed override void Handle(ReceivedMessage received)
    {
        if (ExpectedReceiver == ExpectedReceiverType.ServerOnly && !received.IsServerHandled)
        {
            throw new ExpectedServerException();
        }
        else if (ExpectedReceiver == ExpectedReceiverType.ClientsOnly && received.IsServerHandled)
        {
            throw new ExpectedClientException();
        }

        OnHandleMessage(received);
    }
}