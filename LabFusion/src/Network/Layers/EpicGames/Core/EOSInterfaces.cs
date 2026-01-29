using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Platform;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Provides access to EOS interface instances.
/// </summary>
internal static class EOSInterfaces
{
    public static PlatformInterface Platform { get; private set; }
    public static ConnectInterface Connect { get; private set; }
    public static P2PInterface P2P { get; private set; }
    public static LobbyInterface Lobby { get; private set; }

    public static bool IsInitialized => Platform != null;

    internal static void Initialize(PlatformInterface platform)
    {
        Platform = platform;
        Connect = platform?.GetConnectInterface();
        P2P = platform?.GetP2PInterface();
        Lobby = platform?.GetLobbyInterface();
    }

    internal static void Shutdown()
    {
        Platform?.Release();
        Platform = null;
        Connect = null;
        P2P = null;
        Lobby = null;
    }

    internal static bool ValidateInterfaces()
    {
        return Platform != null && Connect != null && P2P != null && Lobby != null;
    }
}