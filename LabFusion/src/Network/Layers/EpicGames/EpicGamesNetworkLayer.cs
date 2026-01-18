using LabFusion.Data;
using LabFusion.Voice;

namespace LabFusion.Network.EpicGames;

public class EpicGamesNetworkLayer : NetworkLayer
{
    public override string Title => "Epic Online Services";

    public override string Platform => "Epic";
    
    public override bool IsHost => _isServerActive;
    public override bool IsClient => _isConnectionActive;
    
    private INetworkLobby _currentLobby;
    public override INetworkLobby Lobby => _currentLobby;

    private IVoiceManager _voiceManager = null;
    public override IVoiceManager VoiceManager => _voiceManager;

    private IMatchmaker _matchmaker = null;
    public override IMatchmaker Matchmaker => _matchmaker;
    
    protected bool _isServerActive = false;
    protected bool _isConnectionActive = false;
    
    EOSManager eosManager = null;
    EOSAuthManager eosAuthManager = null;
    
    public override bool CheckSupported()
    {
        return true;
    }
    
    public override bool CheckValidation()
    {
        return EOSSDKLoader.HasEOSSDK;
    }

    public override void OnInitializeLayer()
    {
        throw new NotImplementedException();
    }

    public override void OnDeinitializeLayer()
    {
        throw new NotImplementedException();
    }

    public override void LogIn()
    {
        if (eosAuthManager == null)
            eosAuthManager = new EOSAuthManager();
        
        if (eosManager == null)
            eosManager = new EOSManager(eosAuthManager);
        
        NetworkLayerNotifications.SendLoggingInNotification();
        
        
    }

    public override void LogOut()
    {
        
    }

    public override void OnUpdateLayer()
    {
        
    }

    public override string GetUsername(ulong userId)
    {
        return "";
    }

    public override bool IsFriend(ulong userId)
    {
        return true;
    }

    public override void BroadcastMessage(NetworkChannel channel, NetMessage message)
    {
        
    }

    public override void SendToServer(NetworkChannel channel, NetMessage message)
    {
        
    }

    public override void SendFromServer(byte userId, NetworkChannel channel, NetMessage message)
    {
        
    }

    public override void SendFromServer(ulong userId, NetworkChannel channel, NetMessage message)
    {
        
    }

    public override void StartServer()
    {
        
    }

    public override void Disconnect(string reason = "")
    {
        
    }

    public override void DisconnectUser(ulong platformID)
    {
        
    }

    public override string GetServerCode()
    {
        return "";
    }

    public override void RefreshServerCode()
    {
        
    }

    public override void JoinServerByCode(string code)
    {
        
    }
}