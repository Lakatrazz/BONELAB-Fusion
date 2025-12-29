using LabFusion.Network;

namespace LabFusion.Senders;

public static class PermissionSender
{
    public static void SendPermissionRequest(PermissionCommandType type, byte? otherPlayer = null)
    {
        var data = new PermissionCommandRequestData()
        {
            Type = type,
            OtherPlayer = otherPlayer,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.PermissionCommandRequest, CommonMessageRoutes.ReliableToServer);
    }
}
