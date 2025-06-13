using LabFusion.Data;
using LabFusion.Network.Serialization;
using LabFusion.Preferences.Server;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;

namespace LabFusion.Network;

public static class LobbyInfoManager
{
    private static LobbyInfo _lobbyInfo = LobbyInfo.Empty;
    public static LobbyInfo LobbyInfo
    {
        get
        {
            return _lobbyInfo;
        }
        set
        {
            _lobbyInfo = value;

            OnLobbyInfoChanged.InvokeSafe("executing LobbyInfoManager.OnLobbyInfoChanged");
        }
    }

    public static event Action OnLobbyInfoChanged;

    public static void OnInitialize()
    {
        // Hook lobby updates
        MultiplayerHooking.OnMainSceneInitialized += PushLobbyUpdate;
        MultiplayerHooking.OnPlayerJoined += (_) => { PushLobbyUpdate(); };
        MultiplayerHooking.OnPlayerLeft += (_) => { PushLobbyUpdate(); };
        MultiplayerHooking.OnStartedServer += PushLobbyUpdate;
        MultiplayerHooking.OnDisconnected += PushLobbyUpdate;

        SavedServerSettings.OnSavedServerSettingsChanged += PushLobbyUpdate;

        GamemodeManager.OnGamemodeChanged += (_) => { PushLobbyUpdate(); };
    }

    public static void PushLobbyUpdate()
    {
        // Make sure we actually have a Network Layer
        if (NetworkLayerManager.Layer == null)
        {
            LobbyInfo = LobbyInfo.Empty;
            return;
        }

        // If there is no server, empty the lobby info
        if (!NetworkInfo.HasServer)
        {
            LobbyInfo = LobbyInfo.Empty;
            return;
        }

        // We are a client, so we shouldn't override the saved info
        if (NetworkInfo.IsClient)
        {
            return;
        }

        // Write the lobby info
        var info = new LobbyInfo();
        info.WriteLobby();

        LobbyInfo = info;

        // If a server is active, send the info
        if (NetworkInfo.IsHost)
        {
            SendLobbyInfo();
        }
    }

    private static void SendLobbyInfo()
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        var data = ServerSettingsData.Create();

        MessageRelay.RelayNative(data, NativeMessageTag.ServerSettings, CommonMessageRoutes.ReliableToOtherClients);
    }

    internal static void SendLobbyInfo(ulong longId)
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        using var writer = NetWriter.Create();
        var data = ServerSettingsData.Create();
        writer.SerializeValue(ref data);

        using var message = NetMessage.Create(NativeMessageTag.ServerSettings, writer, CommonMessageRoutes.None);
        MessageSender.SendFromServer(longId, NetworkChannel.Reliable, message);
    }
}