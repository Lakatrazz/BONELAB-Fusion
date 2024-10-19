namespace LabFusion.Network;

public interface IMatchmaker
{
    public struct MatchmakerCallbackInfo
    {
        public INetworkLobby[] lobbies;
    }

    void RequestLobbies(Action<MatchmakerCallbackInfo> callback);
}