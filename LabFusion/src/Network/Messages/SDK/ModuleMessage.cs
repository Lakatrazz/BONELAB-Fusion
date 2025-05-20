using LabFusion.SDK.Modules;

namespace LabFusion.Network;

public class ModuleMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.Module;

    protected override bool OnPreRelayMessage(ReceivedMessage received)
    {
        return ModuleMessageHandler.PreRelayMessage(received);
    }

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        ModuleMessageHandler.ReadMessage(received);
    }
}