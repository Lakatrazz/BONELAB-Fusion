﻿using FusionHelper.Network;
using LiteNetLib;
using LiteNetLib.Utils;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{

    internal class ProxyLobbyManager
    {
        private TaskCompletionSource<ulong[]> _lobbyIdSource = null;
        private readonly ProxyNetworkLayer _networkLayer;
        private Dictionary<ulong, TaskCompletionSource<LobbyMetadataInfo>> _metadataInfoRequests = new Dictionary<ulong, TaskCompletionSource<LobbyMetadataInfo>>();

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
                MelonLogger.Msg($"Got LobbyMetadata for {lobbyId}");

                if (!_metadataInfoRequests.ContainsKey(lobbyId))
                {
                    MelonLogger.Error("Got extraneous LobbyMetadata response?");
                    return;
                }

                var tcs = _metadataInfoRequests[lobbyId];

                LobbyMetadataInfo metadataInfo = new LobbyMetadataInfo()
                {
                    LobbyId = packetReader.GetULong(),
                    LobbyName = packetReader.GetString(),
                    HasServerOpen = packetReader.GetBool(),
                    PlayerCount = packetReader.GetInt(),
                    NametagsEnabled = packetReader.GetBool(),
                    Privacy = (ServerPrivacy)packetReader.GetInt(),
                    TimeScaleMode = (Senders.TimeScaleMode)packetReader.GetInt(),

                    MaxPlayers = packetReader.GetInt(),
                    VoicechatEnabled = packetReader.GetBool(),

                    LevelName = packetReader.GetString(),
                    GamemodeName = packetReader.GetString()
                };
                if (Version.TryParse(packetReader.GetString(), out var version))
                {
                    metadataInfo.LobbyVersion = version;
                }
                else
                {
                    metadataInfo.LobbyVersion = new Version(0, 0, 0);
                }

                tcs.SetResult(metadataInfo);
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