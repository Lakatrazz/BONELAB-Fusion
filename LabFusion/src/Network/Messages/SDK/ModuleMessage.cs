using LabFusion.SDK.Modules;

namespace LabFusion.Network;

public class ModuleMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.Module;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        ModuleMessageHandler.ReadMessage(bytes, isServerHandled);
    }
}