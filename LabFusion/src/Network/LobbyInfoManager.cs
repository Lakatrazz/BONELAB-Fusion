using LabFusion.Data;
using LabFusion.Preferences;
using LabFusion.Preferences.Server;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;

namespace LabFusion.Network;

public static class LobbyInfoManager
{
    public static LobbyInfo LobbyInfo { get; internal set; } = new();

    public static event Action OnLobbyInfoChanged;

    public static void OnInitialize()
    {
        // Hook lobby updates
        MultiplayerHooking.OnMainSceneInitialized += PushLobbyUpdate;
        MultiplayerHooking.OnPlayerJoin += (_) => { PushLobbyUpdate(); };
        MultiplayerHooking.OnPlayerLeave += (_) => { PushLobbyUpdate(); };
        MultiplayerHooking.OnStartServer += PushLobbyUpdate;
        MultiplayerHooking.OnDisconnect += PushLobbyUpdate;

        SavedServerSettings.OnSavedServerSettingsChanged += PushLobbyUpdate;

        GamemodeManager.OnGamemodeChanged += (_) => { PushLobbyUpdate(); };
    }

    public static void PushLobbyUpdate()
    {
        // Make sure we actually have a Network Layer
        if (NetworkInfo.CurrentNetworkLayer == null)
        {
            return;
        }

        // We are a client, so we shouldn't override the saved info
        if (NetworkInfo.IsClient)
        {
            return;
        }

        // Write the lobby info
        LobbyInfo = new();
        LobbyInfo.WriteLobby();

        // If a server is active, send the info
        if (NetworkInfo.IsServer)
        {
            SendLobbyInfo();
        }

        OnLobbyInfoChanged.InvokeSafe("executing lobby info changed hook");
    }

    private static void SendLobbyInfo()
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        using var writer = FusionWriter.Create();
        var data = ServerSettingsData.Create();
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.ServerSettings, writer);
        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
    }

    internal static void SendLobbyInfo(ulong longId)
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        using var writer = FusionWriter.Create();
        var data = ServerSettingsData.Create();
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.ServerSettings, writer);
        MessageSender.SendFromServer(longId, NetworkChannel.Reliable, message);
    }
}