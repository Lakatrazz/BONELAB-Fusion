using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionHelper.Network
{
    internal enum MessageTypes
    {
        SteamID = 0,
        OnDisconnected = 1,
        OnMessage = 2,
        GetUsername = 3,
        UnreliableBroadcastToClients = 4,
        ReliableBroadcastToClients = 5,
        UnreliableBroadcastToServer = 6,
        ReliableBroadcastToServer = 7,
        OnConnectionDisconnected = 8,
        OnConnectionMessage = 9,
        JoinServer = 10,
        Disconnect = 11,
        Ping = 12,
    }
}
