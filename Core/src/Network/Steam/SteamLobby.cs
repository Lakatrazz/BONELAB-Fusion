using Steamworks.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public class SteamLobby : INetworkLobby {
        private Lobby _lobby;

        public SteamLobby(Lobby lobby) {
            _lobby = lobby;
        }

        public void SetMetadata(string key, string value) {
            _lobby.SetData(key, value);
        }

        public bool TryGetMetadata(string key, out string value) {
            value = _lobby.GetData(key);
            return !string.IsNullOrWhiteSpace(value);
        }

        public string GetMetadata(string key) { 
            return _lobby.GetData(key);
        }

        public Action CreateJoinDelegate(LobbyMetadataInfo info) {
            if (NetworkInfo.CurrentNetworkLayer is SteamNetworkLayer steamLayer) {
                return () => {
                    steamLayer.JoinServer(info.LobbyId);
                };
            }

            return null;
        }
    }
}
