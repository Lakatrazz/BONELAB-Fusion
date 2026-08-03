using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Senders;

public static class ConnectionSender
{
    public static void SendDisconnect(ClientPlatformID platformID, string reason = "")
    {
        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        using var writer = NetWriter.Create();
        var disconnect = DisconnectMessageData.Create(platformID, reason);
        writer.SerializeValue(ref disconnect);

        using var message = NetMessage.Create(NativeMessageTag.Disconnect, writer, CommonMessageRoutes.None);
        ServerManager.SendToClients(message, NetworkChannel.Reliable);

        NetworkConnectionManager.TimeoutDisconnect(platformID);
    }

    public static void SendConnectionDeny(ClientPlatformID platformID, string reason = "")
    {
        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        using var writer = NetWriter.Create();
        var disconnect = DisconnectMessageData.Create(platformID, reason);
        writer.SerializeValue(ref disconnect);
        
        using var message = NetMessage.Create(NativeMessageTag.Disconnect, writer, CommonMessageRoutes.None);
        ServerManager.SendToClient(message, NetworkChannel.Reliable, platformID);

        NetworkConnectionManager.TimeoutDisconnect(platformID);
    }

    public static void SendConnectionRequest()
    {
        if (!ClientManager.IsClientConnecting)
        {
            FusionLogger.Error("Attempted to send a connection request, but we are not connecting to anyone!");
            return;
        }

        using var writer = NetWriter.Create();

        var data = ConnectionRequestData.Create(FusionMod.Version);
        data.Serialize(writer);

        using NetMessage message = NetMessage.Create(NativeMessageTag.ConnectionRequest, writer, CommonMessageRoutes.None);
        ClientManager.SendToServer(message, NetworkChannel.Reliable);
    }

    public static void SendPlayerCatchup(ClientPlatformID newUser, PlayerID id)
    {
        using var writer = NetWriter.Create();
        var response = ConnectionResponseData.Create(id, false);
        writer.SerializeValue(ref response);

        using var message = NetMessage.Create(NativeMessageTag.ConnectionResponse, writer, CommonMessageRoutes.None);
        ServerManager.SendToClient(message, NetworkChannel.Reliable, newUser);
    }

    public static void SendPlayerJoin(PlayerID id)
    {
        using var writer = NetWriter.Create();
        var response = ConnectionResponseData.Create(id, true);
        writer.SerializeValue(ref response);

        using var message = NetMessage.Create(NativeMessageTag.ConnectionResponse, writer, CommonMessageRoutes.None);
        ServerManager.SendToClients(message, NetworkChannel.Reliable);
    }
}