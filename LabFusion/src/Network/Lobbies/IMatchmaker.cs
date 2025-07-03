namespace LabFusion.Network;

public interface IMatchmaker
{
    public struct MatchmakerCallbackInfo
    {
        public LobbyInfo[] Lobbies;
    }

    public struct LobbyInfo
    {
        public INetworkLobby Lobby;
        public LobbyMetadataInfo Metadata;
    }

    void RequestLobbies(Action<MatchmakerCallbackInfo> callback);

    void RequestLobbiesByCode(string code, Action<MatchmakerCallbackInfo> callback);
}