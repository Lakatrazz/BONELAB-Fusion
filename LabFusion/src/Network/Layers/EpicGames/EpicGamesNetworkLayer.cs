using Epic.OnlineServices;

using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Voice;
using LabFusion.Voice.Unity;

using MelonLoader;

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

    ProductUserId puid => eosAuthManager?.puid;

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
        PlayerIDManager.SetPlatformID(puid.ToString());
        MelonCoroutines.Start(EOSUsernameDeterminer.GetUsernameAsync(s =>
        {
            LocalPlayer.Username = s;
        }));

        HookEOSEvents();

        _voiceManager = new UnityVoiceManager();
        _voiceManager.Enable();

        // _matchmaker = new EOSMatchmaker();
    }

    public override void OnDeinitializeLayer()
    {
        Disconnect();

        _voiceManager.Disable();
        _voiceManager = null;

        _matchmaker = null;

        _currentLobby = null;

        eosManager.ShutdownEOS();

        UnHookEOSEvents();

        eosManager = null;
        eosAuthManager = null;
    }

    public override void LogIn()
    {
        eosAuthManager ??= new EOSAuthManager();

        eosManager ??= new EOSManager(eosAuthManager);

        NetworkLayerNotifications.SendLoggingInNotification();

        MelonCoroutines.Start(eosManager.InitializeAsync(success =>
        {
            if (success)
                InvokeLoggedInEvent();

            else
            {
                FusionLogger.Error("Failed to log in to EOS Network Layer.");
                NetworkLayerNotifications.SendLoginFailedNotification();
            }
        }));
    }

    public override void LogOut()
    {
        InvokeLoggedOutEvent();
    }

    public override void OnUpdateLayer()
    {
        /*
        if (_isConnectionActive)
            EOSSocketHandler.ReceiveMessages();
        */
    }

    public override string GetUsername(string platformId)
    {
        return EOSUsernameDeterminer.GetRandomUsername(platformId);
    }

    // Using device id basically gets rid of all epic account features. Including friends
    public override bool IsFriend(string userId)
    {
        return false;
    }

    public override void BroadcastMessage(NetworkChannel channel, NetMessage message)
    {
        /*
        if (IsHost)
            EOSMessageSender.BroadcastToClients(channel, message);
        else
            EOSMessageSender.BroadcastToServer(channel, message);
        */

    }

    public override void SendToServer(NetworkChannel channel, NetMessage message)
    {
        // EOSMessageSender.BroadcastToServer(channel, message);
    }

    public override void SendFromServer(byte userId, NetworkChannel channel, NetMessage message)
    {
        var playerID = PlayerIDManager.GetPlayerID(userId);
        if (playerID != null)
        {
            SendFromServer(playerID.PlatformID, channel, message);
        }
    }

    public override void SendFromServer(string userId, NetworkChannel channel, NetMessage message)
    {
        // EOSMessageSender.SendFromServer(userId, channel, message);
    }

    public override void StartServer()
    {

    }

    public void JoinServer(string lobbyId)
    {
        // Leave existing server
        if (_isConnectionActive || _isServerActive)
            Disconnect();

        // Do epic lobby connection stuff

        _isServerActive = false;
        _isConnectionActive = true;

        ConnectionSender.SendConnectionRequest();
    }

    public override void Disconnect(string reason = "")
    {

    }

    public override void DisconnectUser(string platformID)
    {
        // Make sure we are hosting a server
        if (!_isServerActive)
        {
            return;
        }

        //EOSConnectionManager.DisconnectUser(platformID);
    }

    public string ServerCode { get; private set; } = null;

    public override string GetServerCode()
    {
        return ServerCode;
    }

    public override void RefreshServerCode()
    {
        ServerCode = RandomCodeGenerator.GetString(8);

        LobbyInfoManager.PushLobbyUpdate();
    }

    public override void JoinServerByCode(string code)
    {
        if (Matchmaker == null)
        {
            return;
        }

#if DEBUG
        FusionLogger.Log($"Searching for servers with code {code}...");
#endif

        Matchmaker.RequestLobbiesByCode(code, (info) =>
        {
            if (info.Lobbies.Length <= 0)
            {
                FusionLogger.Log("No lobbies found with the given code.");
                return;
            }

            JoinServer(info.Lobbies[0].Metadata.LobbyInfo.LobbyID);
        });
    }

    private void HookEOSEvents()
    {
        // Add server hooks
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeave;
        MultiplayerHooking.OnDisconnected += OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged += OnUpdateLobby;
    }

    private void OnPlayerJoin(PlayerID id)
    {
        if (VoiceManager == null)
        {
            return;
        }

        if (!id.IsMe)
        {
            VoiceManager.GetSpeaker(id);
        }
    }

    private void OnPlayerLeave(PlayerID id)
    {
        if (VoiceManager == null)
        {
            return;
        }

        VoiceManager.RemoveSpeaker(id);
    }

    private void OnDisconnect()
    {
        if (VoiceManager == null)
        {
            return;
        }

        VoiceManager.ClearManager();
    }

    public void OnUpdateLobby()
    {
        // Make sure the lobby exists
        if (Lobby == null)
        {
#if DEBUG
            FusionLogger.Warn("Tried updating the EOS lobby, but it was null!");
#endif
            return;
        }

        // Write active info about the lobby
        LobbyMetadataSerializer.WriteInfo(Lobby);
    }

    private void UnHookEOSEvents()
    {
        // Remove server hooks
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft -= OnPlayerLeave;
        MultiplayerHooking.OnDisconnected -= OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged -= OnUpdateLobby;
    }
}