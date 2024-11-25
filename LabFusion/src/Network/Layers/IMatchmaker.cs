namespace LabFusion.Network;

public interface IMatchmaker
{
    public struct MatchmakerCallbackInfo
    {
        public LobbyInfo[] lobbies;
    }

    public struct LobbyInfo
    {
        public INetworkLobby lobby;
        public LobbyMetadataInfo metadata;
    }

    void RequestLobbies(Action<MatchmakerCallbackInfo> callback);
}