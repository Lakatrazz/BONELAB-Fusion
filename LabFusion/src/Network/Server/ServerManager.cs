using LabFusion.Player;

using System.Buffers;

namespace LabFusion.Network;

/// <summary>
/// Manages state and data transfer for the server.
/// </summary>
public static class ServerManager
{
    /// <summary>
    /// Returns true if a server is currently running on this instance.
    /// <para>This will not return true if this instance is only a client that has joined the server.
    /// To check if a server exists at all, see <see cref="NetworkManager.HasServer"/>.</para>
    /// </summary>
    public static bool IsServerRunning => NetworkInfo.IsHost;

    /// <summary>
    /// Sends a message from the server to a specific client.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    /// <param name="client"></param>
    public static void SendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID client)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var layer = NetworkLayerManager.Layer;

        if (layer == null)
        {
            return;
        }

        NetworkInfo.BytesUp += message.Length;

        layer.ServerSendToClient(message, channel, client);
    }

    /// <summary>
    /// Sends a message from the server to multiple clients.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    public static void SendToClients(NetMessage message, NetworkChannel channel, Span<ClientPlatformID> clients)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var layer = NetworkLayerManager.Layer;

        if (layer == null)
        {
            return;
        }

        NetworkInfo.BytesUp += message.Length;

        layer.ServerSendToClients(message, channel, clients);
    }

    /// <summary>
    /// Sends a message from the server to all connected clients.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    public static void SendToClients(NetMessage message, NetworkChannel channel)
    {
        var playerIDs = PlayerIDManager.PlayerIDs;
        int idCount = playerIDs.Count;

        var clients = RentClients(playerIDs);

        SendToClients(message, channel, new Span<ClientPlatformID>(clients, 0, idCount));

        ArrayPool<ClientPlatformID>.Shared.Return(clients);
    }

    /// <summary>
    /// Sends a message from the server to all connected clients except for a specified client.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    /// <param name="client"></param>
    public static void SendToClientsExcept(NetMessage message, NetworkChannel channel, ClientPlatformID client)
    {
        var playerIDs = PlayerIDManager.PlayerIDs.Where(playerID => playerID.PlatformID != client);
        int idCount = playerIDs.Count();

        var clients = RentClients(playerIDs);

        SendToClients(message, channel, new Span<ClientPlatformID>(clients, 0, idCount));

        ArrayPool<ClientPlatformID>.Shared.Return(clients);
    }

    private static ClientPlatformID[] RentClients(IEnumerable<PlayerID> playerIDs)
    {
        int idCount = playerIDs.Count();

        ClientPlatformID[] clients = ArrayPool<ClientPlatformID>.Shared.Rent(idCount);

        int index = 0;

        foreach (var id in playerIDs)
        {
            clients[index++] = id.PlatformID;
        }

        return clients;
    }
}
