using LabFusion.Utilities;
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
            if (!info.ClientHasLevel) {
                return () => {
                    FusionNotifier.Send(new FusionNotification() {
                        title = "Failed to Join",
                        showTitleOnPopup = true,
                        isMenuItem = false,
                        isPopup = true,
                        message = $"You do not have the map {info.LevelName} installed!",
                        popupLength = 6f,
                    });
                };
            }

            if (NetworkInfo.CurrentNetworkLayer is SteamNetworkLayer steamLayer) {
                return () => {
                    steamLayer.JoinServer(info.LobbyId);
                };
            }

            return null;
        }
    }
}
