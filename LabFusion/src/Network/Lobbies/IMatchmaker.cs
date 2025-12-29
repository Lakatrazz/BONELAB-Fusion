namespace LabFusion.Network;

public interface IMatchmaker
{
    public struct MatchmakerCallbackInfo
    {
        public static readonly MatchmakerCallbackInfo Empty = new() { Lobbies = Array.Empty<LobbyInfo>() };

        public LobbyInfo[] Lobbies;
    }

    public struct LobbyInfo
    {
        public INetworkLobby Lobby;
        public LobbyMetadataInfo Metadata;
    }

    /// <summary>
    /// Requests a list of all possible lobbies and invokes a callback when finished.
    /// </summary>
    /// <param name="callback"></param>
    void RequestLobbies(Action<MatchmakerCallbackInfo> callback);

    /// <summary>
    /// Requests a list of lobbies given a set of filters and invokes a callback when finished.
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="callback"></param>
    void RequestLobbies(MatchmakerFilters filters, Action<MatchmakerCallbackInfo> callback);

    /// <summary>
    /// Requests a list of lobbies given a code and invokes a callback when finished.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="callback"></param>
    void RequestLobbiesByCode(string code, Action<MatchmakerCallbackInfo> callback);
}