using Steamworks.Data;

namespace LabFusion.Network
{
    public class SteamLobby : NetworkLobby
    {
        private Lobby _lobby;

        public SteamLobby(Lobby lobby)
        {
            _lobby = lobby;
        }

        public override void SetMetadata(string key, string value)
        {
            _lobby.SetData(key, value);
            SaveKey(key);
        }

        public override bool TryGetMetadata(string key, out string value)
        {
            value = _lobby.GetData(key);
            return !string.IsNullOrWhiteSpace(value);
        }

        public override string GetMetadata(string key)
        {
            return _lobby.GetData(key);
        }

        public override Action CreateJoinDelegate(ulong lobbyId)
        {
            if (NetworkInfo.CurrentNetworkLayer is SteamNetworkLayer steamLayer)
            {
                return () =>
                {
                    steamLayer.JoinServer(lobbyId);
                };
            }

            return null;
        }
    }
}
