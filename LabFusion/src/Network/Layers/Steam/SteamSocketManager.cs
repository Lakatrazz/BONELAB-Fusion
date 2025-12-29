using LabFusion.Player;
using LabFusion.Senders;

using Steamworks;
using Steamworks.Data;

namespace LabFusion.Network;

public class SteamSocketManager : SocketManager
{
    public Dictionary<ulong, Connection> ConnectedSteamIDs = new();

    public void DisconnectUser(ulong steamID)
    {
        if (!ConnectedSteamIDs.TryGetValue(steamID, out var connection))
        {
            return;
        }

        connection.Close();
    }

    public override void OnConnecting(Connection connection, ConnectionInfo data)
    {
        base.OnConnecting(connection, data);
        connection.Accept();
    }

    public override void OnConnected(Connection connection, ConnectionInfo data)
    {
        base.OnConnected(connection, data);

        // If we aren't in the connected list yet, this is likely us
        // SteamID is always 0 here
        if (!ConnectedSteamIDs.ContainsKey(SteamClient.SteamId))
        {
            ConnectedSteamIDs[SteamClient.SteamId] = connection;
        }
    }

    public override void OnDisconnected(Connection connection, ConnectionInfo data)
    {
        base.OnDisconnected(connection, data);

        // Remove connection from list
        var pair = ConnectedSteamIDs.First((p) => p.Value.Id == connection.Id);
        var platformID = pair.Key;

        ConnectedSteamIDs.Remove(pair.Key);

        // Make sure the user hasn't previously disconnected
        if (PlayerIDManager.HasPlayerID(platformID))
        {
            // Update the mod so it knows this user has left
            InternalServerHelpers.OnPlayerLeft(pair.Key);

            // Send disconnect notif to everyone
            ConnectionSender.SendDisconnect(platformID);
        }
    }

    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);

        var platformID = identity.steamid;

        ConnectedSteamIDs[platformID] = connection;

        SteamSocketHandler.OnSocketMessageReceived(data, size, true, platformID);
    }
}
