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
    public static readonly SocketId SocketId = new() { SocketName = "FusionSocket" };

    private static EOSMessenger _instance;
    private static EOSMessenger Instance => _instance ??= new EOSMessenger();

    private readonly EOSBufferPool _bufferPool;
    private readonly EOSPacketSender _sender;
    private readonly EOSPacketReceiver _receiver;

    private EOSMessenger()
    {
        _bufferPool = new EOSBufferPool();
        _sender = new EOSPacketSender(_bufferPool, SocketId);
        _receiver = new EOSPacketReceiver(_bufferPool, SocketId);
    }

    public static void ReceiveMessages()
    {
        Instance._receiver.ReceiveMessages();
    }

    public static Result SendPacket(ProductUserId userId, NetMessage message, NetworkChannel channel, bool isServerHandled)
    {
        return Instance._sender.Send(userId, message, channel, isServerHandled);
    }

    public static void BroadcastToClients(NetworkChannel channel, NetMessage message)
    {
        var lobbyDetails = EOSLobbyManager.CurrentLobbyDetails;
        if (lobbyDetails == null)
            return;

        var countOptions = new LobbyDetailsGetMemberCountOptions();
        uint memberCount = lobbyDetails.GetMemberCount(ref countOptions);

        for (uint i = 0; i < memberCount; i++)
        {
            var memberOptions = new LobbyDetailsGetMemberByIndexOptions { MemberIndex = i };
            var memberId = lobbyDetails.GetMemberByIndex(ref memberOptions);

            if (memberId != null)
            {
                SendPacket(memberId, message, channel, isServerHandled: false);
            }
        }
    }

    public static void BroadcastToServer(NetworkChannel channel, NetMessage message)
    {
        var hostId = GetHostProductUserId();
        if (hostId != null)
        {
            SendPacket(hostId, message, channel, isServerHandled: true);
        }
    }

    public static void SendFromServer(string userId, NetworkChannel channel, NetMessage message)
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

    public static void Reset()
    {
        _instance = null;
    }
}