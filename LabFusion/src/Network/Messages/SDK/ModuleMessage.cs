using LabFusion.SDK.Modules;

namespace LabFusion.Network;

public class ModuleMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.Module;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        ModuleMessageHandler.ReadMessage(received);
    }
}