using LabFusion.Network;

namespace LabFusion.SDK.Lobbies
{
    public delegate bool GenericLobbyDelegate(INetworkLobby lobby, LobbyMetadataInfo info);

    public class GenericLobbyFilter : ILobbyFilter
    {
        public GenericLobbyDelegate onFilter;

        public string title;

        private bool _active = false;

        public GenericLobbyFilter(string title, GenericLobbyDelegate onFilter)
        {
            this.title = title;
            this.onFilter = onFilter;
        }

        public bool FilterLobby(INetworkLobby lobby, LobbyMetadataInfo info)
        {
            return onFilter(lobby, info);
        }

        public string GetTitle()
        {
            return title;
        }

        public bool IsActive()
        {
            return _active;
        }

        public void SetActive(bool active)
        {
            _active = active;
        }
    }
}
