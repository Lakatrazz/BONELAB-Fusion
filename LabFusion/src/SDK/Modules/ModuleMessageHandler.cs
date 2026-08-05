using LabFusion.Network;

namespace LabFusion.SDK.Modules;

public abstract class ModuleMessageHandler : MessageHandler
{
    /// <summary>
    /// The internal 64 bit tag used to identify the module message across clients and the server.
    /// </summary>
    public long? Tag { get; internal set; } = null;

    /// <summary>
    /// Always false for module messages, as they should only be sendable when the connection has been established and the client is in the server.
    /// </summary>
    public sealed override bool AllowConnectingClients => base.AllowConnectingClients;

    public sealed override void Handle(ReceivedMessage received)
    {
        CheckExpectedReceiver(received);

        OnHandleMessage(received);
    }
}