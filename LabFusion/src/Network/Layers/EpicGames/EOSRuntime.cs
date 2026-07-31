using System.Collections;

namespace LabFusion.Network;

internal class EOSRuntime
{
    internal EOSPlatform Platform { get; private set; }
    internal EOSConnect Connect { get; private set; }
    internal EOSP2P P2P { get; private set; }
    internal EOSLobby Lobby { get; private set; }

    internal IEnumerator InitializeAsync(Action<bool> onComplete)
    {
        Platform = new EOSPlatform();
        bool platformSuccess = false;
        yield return Platform.InitializeAsync((success) => platformSuccess = success);
        if (!platformSuccess)
        {
            onComplete?.Invoke(false);
            yield break;
        }

        Connect = new EOSConnect(Platform.PlatformInterface.GetConnectInterface());
        bool connectSuccess = false;
        yield return Connect.InitializeAsync((success) => connectSuccess = success);
        if (!connectSuccess)
        {
            onComplete?.Invoke(false);
            yield break;
        }

        P2P = new EOSP2P(this, Platform.PlatformInterface.GetP2PInterface(), Connect.LocalUserId);
        bool p2pSuccess = false;
        yield return P2P.InitializeAsync((success) => p2pSuccess = success);
        if (!p2pSuccess)
        {
            onComplete?.Invoke(false);
            yield break;
        }

        Lobby = new EOSLobby(this, Platform.PlatformInterface.GetLobbyInterface(), Connect.LocalUserId);
        bool lobbySuccess = false;
        yield return Lobby.InitializeAsync((success) => lobbySuccess = success);
        if (!lobbySuccess)
        {
            onComplete?.Invoke(false);
            yield break;
        }

        onComplete?.Invoke(true);
    }

    internal void Shutdown()
    {
        Lobby?.Shutdown();
        P2P?.Shutdown();
        Connect?.Shutdown();
        Platform?.Shutdown();
    }
}