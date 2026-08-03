using LabFusion.Utilities;

namespace LabFusion.Network;

/// <summary>
/// An empty networking layer for fallback. This does not implement any multiplayer functionality.
/// </summary>
public class EmptyNetworkLayer : NetworkLayer
{
    public override string Title => "Empty";

    public override string Platform => "Empty";

    public override bool IsServerRunning => false;

    public override bool IsClientConnected => false;

    public override ServerID RunningServerID => throw new NotImplementedException();

    public override ServerID ConnectedServerID => throw new NotImplementedException();

    public override void Disconnect(string reason = "") { }

    public override void DisconnectUser(ClientPlatformID platformID) { }

    public override void StartServer() { }

    public override bool CheckSupported()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }

    public override bool CheckValidation()
    {
        return true;
    }

    public override void OnInitializeLayer()
    {
        FusionLogger.Log("Initialized mod with an empty networking layer!", ConsoleColor.Magenta);
#if DEBUG
        FusionLogger.Log("This is for debugging purposes only, and will not allow multiplayer!", ConsoleColor.Magenta);
#else
        FusionLogger.Log("This usually means all other network layers failed to initialize, or you selected Empty in the settings.", ConsoleColor.Magenta);
#endif
    }

    public override void OnDeinitializeLayer() 
    {
    }

    public override void LogIn()
    {
        throw new NotImplementedException();
    }

    public override void LogOut()
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

    public override void ClientSendToServer(NetMessage message, NetworkChannel channel)
    {
        throw new NotImplementedException();
    }

    public override void StopServer()
    {
        throw new NotImplementedException();
    }

    public override void ServerDisconnectClient(ClientPlatformID client)
    {
        throw new NotImplementedException();
    }

    public override void ConnectToServer(ServerID server)
    {
        throw new NotImplementedException();
    }

    public override void ClientDisconnectFromServer()
    {
        throw new NotImplementedException();
    }
}