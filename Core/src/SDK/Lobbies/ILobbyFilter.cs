using LabFusion.Network;

namespace LabFusion.SDK.Lobbies
{
    public interface ILobbyFilter
    {
        string GetTitle();

        bool IsActive();

        void SetActive(bool active);

        bool FilterLobby(INetworkLobby lobby, LobbyMetadataInfo info);
    }
}
