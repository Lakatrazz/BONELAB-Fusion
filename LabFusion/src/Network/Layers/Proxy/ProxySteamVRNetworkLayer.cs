
namespace LabFusion.Network.Proxy;

public sealed class ProxySteamVRNetworkLayer : ProxyNetworkLayer
{
    public override uint ApplicationID => SteamVRNetworkLayer.SteamVRId;

    public override string Title => "Proxy SteamVR";

    public override string Platform => "Steam";

    public override void ClientSendToServer(NetMessage message, NetworkChannel channel)
    {
        throw new NotImplementedException();
    }

    public override void ServerSendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID clientPlatformID)
    {
        throw new NotImplementedException();
    }

    public override void ServerSendToClients(NetMessage message, NetworkChannel channel, Span<ClientPlatformID> clientPlatformIDs)
    {
        throw new NotImplementedException();
    }
}
