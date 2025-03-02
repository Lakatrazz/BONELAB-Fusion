using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Senders
{
    public static class PermissionSender
    {
        public static void SendPermissionRequest(PermissionCommandType type, byte? otherPlayer = null)
        {
            var data = PermissionCommandRequestData.Create(PlayerIdManager.LocalSmallId, type, otherPlayer);

            MessageRelay.RelayNative(data, NativeMessageTag.PermissionCommandRequest, NetworkChannel.Reliable, RelayType.ToServer);
        }
    }
}
