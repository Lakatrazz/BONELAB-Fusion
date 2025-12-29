using LabFusion.Network;

namespace LabFusion.SDK.Modules;

public abstract class ModuleMessageHandler : MessageHandler
{
    internal long? _tag = null;
    public long? Tag => _tag;

    public sealed override void Handle(ReceivedMessage received)
    {
        CheckExpectedConditions(received);

        OnHandleMessage(received);
    }
}