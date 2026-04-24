using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.P2P;
using LabFusion.Player;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// High-level messaging for EOS P2P.
/// </summary>
internal class EOSMessenger
{
    internal static readonly SocketId SocketId = new() { SocketName = "FusionSocket" };

    private static EOSMessenger _instance;
    private static EOSMessenger Instance => _instance ??= new EOSMessenger();

    private readonly EOSBufferPool _bufferPool;
    private readonly EOSPacketSender _sender;
    private readonly EOSPacketReceiver _receiver;
    private readonly HashSet<ProductUserId> _connectedClients = new();

    private EOSMessenger()
    {
        _bufferPool = new EOSBufferPool();
        _sender = new EOSPacketSender(_bufferPool, SocketId);
        _receiver = new EOSPacketReceiver(_bufferPool, SocketId);
    }

    internal static void ReceiveMessages()
    {
        Instance._receiver.ReceiveMessages();
    }

    internal static Result SendPacket(ProductUserId userId, NetMessage message, NetworkChannel channel, bool isServerHandled)
    {
        return Instance._sender.Send(userId, message, channel, isServerHandled);
    }
    
    internal static void AddConnectedClient(ProductUserId userId)
    {
        if (userId != null)
            Instance._connectedClients.Add(userId);
    }
    
    internal static void RemoveConnectedClient(ProductUserId userId)
    {
        if (userId != null)
            Instance._connectedClients.Remove(userId);
    }
    
    internal static void ClearConnectedClients()
    {
        Instance._connectedClients.Clear();
    }

    internal static void BroadcastToClients(NetworkChannel channel, NetMessage message)
    {
        foreach (var clientId in Instance._connectedClients)
        {
            SendPacket(clientId, message, channel, isServerHandled: false);
        }
    }

    internal static void BroadcastToServer(NetworkChannel channel, NetMessage message)
    {
        var hostId = GetHostProductUserId();
        if (hostId != null)
        {
            SendPacket(hostId, message, channel, isServerHandled: true);
        }
    }

    internal static void SendFromServer(string userId, NetworkChannel channel, NetMessage message)
    {
        var targetId = ProductUserId.FromString(userId);
        if (targetId != null)
        {
            SendPacket(targetId, message, channel, isServerHandled: false);
        }
    }

    private static ProductUserId GetHostProductUserId()
    {
        var hostInfo = PlayerIDManager.GetHostID();
        if (hostInfo != null)
        {
            return ProductUserId.FromString(hostInfo.PlatformID);
        }

        var lobbyDetails = EOSLobbyManager.CurrentLobbyDetails;
        if (lobbyDetails != null)
        {
            var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            return lobbyDetails.GetLobbyOwner(ref ownerOptions);
        }

        return null;
    }

    internal static void Reset()
    {
        _instance = null;
    }
}