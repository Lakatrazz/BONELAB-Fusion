using Steamworks;
using Steamworks.Data;

using LabFusion.Utilities;

namespace LabFusion.Network
{
    public class SteamConnectionManager : ConnectionManager
    {
        public override void OnConnected(ConnectionInfo info)
        {
            base.OnConnected(info);
        }

        public override void OnConnecting(ConnectionInfo info)
        {
            base.OnConnecting(info);
        }

        public override void OnDisconnected(ConnectionInfo info)
        {
            base.OnDisconnected(info);

            NetworkHelper.Disconnect();

#if DEBUG
            FusionLogger.Log("Client was disconnected.");
#endif
        }

        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            base.OnMessage(data, size, messageNum, recvTime, channel);

            var steamID = ConnectionInfo.Identity.SteamId;
            ulong? platformID = steamID.IsValid ? steamID.Value : null;

            SteamSocketHandler.OnSocketMessageReceived(data, size, false, platformID);
        }
    }
}
