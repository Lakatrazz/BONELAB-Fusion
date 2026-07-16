using LabFusion.SDK.Modules;

namespace LabFusion.Network;

public class ModuleMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.Module;

    // This is set to true here because it'll be checked for in the module message itself as well
    // By leaving it true here, the creator of the module message can specify it to be true themselves, if needed
    public override bool AllowDirectRelay => true;

    protected override bool OnPreRelayMessage(ReceivedMessage received)
    {
        return ModuleMessageManager.PreRelayMessage(received);
    }

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        ModuleMessageManager.ReadMessage(received);
    }
}