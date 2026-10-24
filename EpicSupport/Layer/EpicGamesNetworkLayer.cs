using Epic.OnlineServices;
using LabFusion.Network;
using LabFusion.Utilities;
using MarrowFusion.Epic.Utilities;

namespace MarrowFusion.Epic;

// TODO: Add on pre client connection stuff
// - Check if logged in via epic
// - Check for active sanctions

public class EpicGamesNetworkLayer : NetworkLayer
{
    public override string Title => "Epic Online Services";
    
    public override string Platform => "Epic";
    
    public override bool IsServerRunning => Runtime?.P2P?.Server?.IsRunning ?? false;

    public override ServerID RunningServerID => _runningServerID;

    public override bool IsClientConnected => Runtime?.P2P?.Client?.IsConnected ?? false;

    public override ClientPlatformID? ClientID => new(ClientProductUserId?.ToString());

    public override ServerID ConnectedServerID => _connectedServerID;

    public override NetworkLobby Lobby => Runtime?.Lobby?.CurrentLobby;

    public override Matchmaker Matchmaker => _matchmaker;
    
    public EpicAccountId ClientEpicAccountId { get; private set; }
    public ProductUserId ClientProductUserId { get; private set; }
    
    private ServerID _runningServerID = ServerID.Empty;
    private ServerID _connectedServerID = ServerID.Empty;

    private Matchmaker _matchmaker = null;
    
    internal EOSRuntime Runtime;

    public override bool CheckSupported() => true;

    public override bool CheckValidation() => EpicResourceManager.IsResourcesAvailable();

    public override void SendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID clientPlatformID)
    {
        if (!IsServerRunning)
        {
            return;
        }
        
        Runtime.P2P.Sender.SendToClient(message, channel, clientPlatformID);
    }

    public override void SendToClients(NetMessage message, NetworkChannel channel, ReadOnlySpan<ClientPlatformID> clientPlatformIDs)
    {
        if (!IsServerRunning)
        {
            return;
        }
        
        Runtime.P2P.Sender.SendToClients(message, channel, clientPlatformIDs);
    }

    public override void SendToServer(NetMessage message, NetworkChannel channel)
    {
        if (!IsClientConnected)
        {
            return;
        }
        
        Runtime.P2P.Sender.SendToServer(message, channel);
    }

    protected override async Task<bool> TryLogInAsync(CancellationToken cancellationToken)
    {
        var loadResourcesTask = await ThreadHelper.RunOnMainThreadAsTask(EpicResourceManager.TryLoadResources);

        if (!loadResourcesTask)
        {
            return false;
        }

        Runtime = new EOSRuntime();

        Task<bool> initializationTask = null;

        await ThreadHelper.RunOnMainThreadAsTask(() =>
        {
            initializationTask = Runtime.InitializeAsync();
        });

        return await initializationTask;
    }

    protected override async Task<bool> TryLogOutAsync(CancellationToken cancellationToken)
    {
        await ThreadHelper.RunOnMainThreadAsTask(() =>
        {
            Runtime.Shutdown();
        });
        
        return true;
    }

    protected override async Task<bool> TryStartServerAsync(CancellationToken cancellationToken)
    {
        await ThreadHelper.RunOnMainThreadAsTask(() =>
        {
            Runtime.Lobby.Create();
            Runtime.P2P.Server.Start();
        });
        
        _runningServerID = new ServerID(ClientProductUserId.ToString());

        TryRefreshServerCode();

        return true;
    }

    protected override async Task<bool> TryStopServerAsync(CancellationToken cancellationToken)
    {
        await ThreadHelper.RunOnMainThreadAsTask(() =>
        {
            Runtime.Lobby.Destroy();
            Runtime.P2P.Server.Stop();
        });
        
        _runningServerID = ServerID.Empty;

        return true;
    }

    protected override async Task<bool> TryDisconnectClientAsync(ClientPlatformID client, CancellationToken cancellationToken)
    {
        ProductUserId targetUserId = ProductUserId.FromString(client.ToString());

        var server = Runtime.P2P.Server;
        
        await ThreadHelper.RunOnMainThreadAsTask(() => server.DisconnectPeer(targetUserId));
        
        return true;
    }

    protected override async Task<bool> TryConnectToServerAsync(ServerID server, CancellationToken cancellationToken)
    {
        ProductUserId serverUserId = ProductUserId.FromString(server.ToString());
        
        var client = Runtime.P2P.Client;
        
        await ThreadHelper.RunOnMainThreadAsTask(() => client.Connect(serverUserId));

        while (client.IsConnecting)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await ThreadHelper.RunOnMainThreadAsTask(client.Disconnect);
                return false;
            }
            
            await Task.Delay(50, CancellationToken.None);
        }
        
        if (!client.IsConnected)
        {
            return false;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            client.Disconnect();
            return false;
        }
        
        _connectedServerID = server;

        return true;
    }

    protected override async Task<bool> TryDisconnectFromServerAsync(CancellationToken cancellationToken)
    {
        var client = Runtime.P2P.Client;
        
        if (client.IsConnected)
        {
            await ThreadHelper.RunOnMainThreadAsTask(client.Disconnect);
        }
        
        while (client.IsConnected)
        {
            await Task.Delay(50, CancellationToken.None);
        }
        
        _connectedServerID = ServerID.Empty;

        return true;
    }

    protected override async Task<string> TryGetLocalUsernameAsync()
    {
        var task = await ThreadHelper.RunOnMainThreadAsTask(() => Runtime.UserInfo.GetDisplayNameAsync(ClientEpicAccountId));

        return await task;
    }

    protected override async Task<string> TryGetClientUsernameAsync(ClientPlatformID client)
    {
        var task = await ThreadHelper.RunOnMainThreadAsTask(() => Runtime.UserInfo.GetDisplayNameAsync(ProductUserId.FromString(client.ToString())));

        return await task;
    }

    protected override void OnInitialize()
    {
        ClientEpicAccountId = Runtime.Auth.LocalAccountId;
        ClientProductUserId = Runtime.Connect.LocalUserId;
        
        EpicModule.Logger.Log($"{Platform} initialized with ProductUserId {ClientID}!");

        _matchmaker = new EpicMatchmaker(Runtime);
    }

    protected override void OnDeinitialize()
    {
        _matchmaker = null;
    }

    protected override void OnTick()
    {
        Runtime?.Tick();
    }
}