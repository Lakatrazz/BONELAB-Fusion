using FusionHelper.Network;

using LabFusion.Utilities;

using LiteNetLib;
using LiteNetLib.Utils;

using MelonLoader;

namespace LabFusion.Network
{

    internal class ProxyLobbyManager
    {
        private TaskCompletionSource<ulong[]> _lobbyIdSource = null;
        private readonly ProxyNetworkLayer _networkLayer;
        private readonly Dictionary<ulong, TaskCompletionSource<LobbyMetadataInfo>> _metadataInfoRequests = new();

        internal ProxyLobbyManager(ProxyNetworkLayer networkLayer)
        {
            _networkLayer = networkLayer;
        }

        internal void HandleLobbyMessage(MessageTypes messageType, NetPacketReader packetReader)
        {
            if (messageType == MessageTypes.LobbyIds)
            {
                MelonLogger.Msg("Got LobbyIds");
                if (_lobbyIdSource == null)
                {
                    MelonLogger.Error("Got extraneous RequestLobbies response?");
                    return;
                }

                // Got a lobby response from the server, read the SteamIDs
                uint numLobbyIds = packetReader.GetUInt();

                ulong[] ids = new ulong[numLobbyIds];

                for (uint i = 0; i < numLobbyIds; i++)
                {
                    ids[i] = packetReader.GetULong();
                }

                // Finish the task
                _lobbyIdSource.SetResult(ids);
                _lobbyIdSource = null;
            }

            if (messageType == MessageTypes.LobbyMetadata)
            {
                ulong lobbyId = packetReader.GetULong();
                FusionLogger.Log($"Got LobbyMetadata for {lobbyId}");

                if (!_metadataInfoRequests.ContainsKey(lobbyId))
                {
                    FusionLogger.Error("Got extraneous LobbyMetadata response?");
                    return;
                }

                var tcs = _metadataInfoRequests[lobbyId];

                ProxyNetworkLobby lobby = new();

                // Get the amount of keys
                int keyCount = packetReader.GetInt();

                // Read key array
                for (var i = 0; i < keyCount; i++)
                {
                    // In order, key then value
                    string key = packetReader.GetString();
                    string value = packetReader.GetString();

                    lobby.CacheMetadata(key, value);
                }

                LobbyMetadataInfo info = LobbyMetadataHelper.ReadInfo(lobby);

                tcs.SetResult(info);
                _metadataInfoRequests.Remove(lobbyId);
            }
        }

        public Task<ulong[]> RequestLobbyIds()
        {
            _lobbyIdSource = new TaskCompletionSource<ulong[]>();
            _networkLayer.SendToProxyServer(MessageTypes.LobbyIds);

            return _lobbyIdSource.Task;
        }

        public Task<LobbyMetadataInfo> RequestLobbyMetadataInfo(ulong lobbyId)
        {
            var tcs = new TaskCompletionSource<LobbyMetadataInfo>();
            _metadataInfoRequests.Add(lobbyId, tcs);
            NetDataWriter writer = ProxyNetworkLayer.NewWriter(MessageTypes.LobbyMetadata);
            writer.Put(lobbyId);
            _networkLayer.SendToProxyServer(writer);

            return tcs.Task;
        }
    }
}
