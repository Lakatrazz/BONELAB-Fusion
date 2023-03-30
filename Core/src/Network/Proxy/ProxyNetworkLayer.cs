﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Preferences;

using SLZ.Rig;

//using Steamworks;
//using Steamworks.Data;

using UnityEngine;

using Color = UnityEngine.Color;

using MelonLoader;

using System.Windows.Forms;

using LabFusion.Senders;
using LabFusion.BoneMenu;

using System.IO;

using UnhollowerBaseLib;
using System.Net;
using Steamworks;
using FusionHelper.Network;
using BoneLib;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;

namespace LabFusion.Network
{
    public class ProxyNetworkLayer : NetworkLayer
    {
        internal static ProxyNetworkLayer Instance { get; private set; }

        internal override bool IsServer => _isServerActive;
        internal override bool IsClient => _isConnectionActive;

        public SteamId SteamId;

        protected bool _isServerActive = false;
        protected bool _isConnectionActive = false;

        protected ulong _targetServerId;

        protected string _targetJoinId;

        protected bool _isInitialized = false;

        private NetManager client;
        private NetPeer serverConnection;
        private ProxyLobbyManager _lobbyManager;

        internal override void OnInitializeLayer()
        {
            Instance = this;

            EventBasedNetListener listener = new EventBasedNetListener();
            client = new NetManager(listener)
            {
                UnconnectedMessagesEnabled = true,
                BroadcastReceiveEnabled = true,
                DisconnectOnUnreachable = true,
                DisconnectTimeout = 10000,
                PingInterval = 5000,
            };
            listener.NetworkReceiveEvent += EvaluateMessage;
            listener.PeerConnectedEvent += (peer) =>
            {
                serverConnection = peer;
                NetDataWriter writer = NewWriter(MessageTypes.SteamID);
                NetworkLayerType layer = FusionPreferences.ClientSettings.NetworkLayerType.GetValue();

                int appId;
                switch (layer)
                {
                    case NetworkLayerType.PROXY_STEAM_VR:
                        appId = SteamVRNetworkLayer.SteamVRId;
                        break;
                    case NetworkLayerType.PROXY_SPACEWAR:
                        appId = SpacewarNetworkLayer.SpacewarId;
                        break;
                    default:
                        FusionLogger.Error("Attempted to initialize ProxyNetworkLayer without a known ApplicationID!");
                        appId = 0;
                        break;
                }
                listener.PeerDisconnectedEvent += (peer, disconnectInfo) => {
                    FusionLogger.Error("Proxy has disconnected, restarting server discovery!");
                    serverConnection = null;
                    MelonCoroutines.Start(DiscoverServer());
                };

                writer.Put(appId);
                SendToProxyServer(writer);
            };

            listener.NetworkReceiveUnconnectedEvent += (endPoint, reader, messageType) =>
            {
                if (reader.TryGetString(out string data) && data == "YOU_FOUND_ME")
                {
                    FusionLogger.Log("Found the proxy server!");
                    client.Connect(endPoint, "ProxyConnection");
                }

                reader.Recycle();
            };

            client.Start();
            FusionLogger.Log("Beginning proxy discovery...");
            MelonCoroutines.Start(DiscoverServer());

            _lobbyManager = new ProxyLobbyManager(this);
        }

        internal IEnumerator DiscoverServer()
        {
            int port = FusionPreferences.ClientSettings.ProxyPort.GetValue();
            if (!(port >= 1024 && port <= 65535))
            {
                FusionLogger.Error("Custom port is invalid, using default! (28430)");
                port = 28340;
            }

            float timeElapsed;

            NetDataWriter writer = new NetDataWriter();
            writer.Put("FUSION_SERVER_DISCOVERY");

            while (serverConnection == null)
            {
                timeElapsed = 0;
                client.SendBroadcast(writer, port);

                while (timeElapsed < 5)
                {
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }
            }
        }

        internal void EvaluateMessage(NetPeer fromPeer, NetPacketReader dataReader, byte channel, DeliveryMethod deliveryMethod)
        {
            ulong id = dataReader.GetByte();
            switch (id)
            {
                case (ulong)MessageTypes.Ping:
                    {
                        double theTime = dataReader.GetDouble();
                        double curTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                        FusionLogger.Log("Server -> Client = " + (curTime - theTime) + " ms.");
                        NetDataWriter writer = NewWriter(MessageTypes.Ping);
                        writer.Put(curTime);
                        SendToProxyServer(writer);
                        break;
                    }
                case (ulong)MessageTypes.SteamID:
                    {
                        SteamId = new SteamId()
                        {
                            Value = dataReader.GetULong()
                        };

                        if (SteamId.Value == 0)
                        {
                            FusionLogger.Error("Steamworks failed to initialize!");
                            break;
                        }

                        PlayerIdManager.SetLongId(SteamId.Value);
                        NetDataWriter writer = NewWriter(MessageTypes.GetUsername);
                        writer.Put(SteamId.Value);
                        SendToProxyServer(writer);

                        FusionLogger.Log($"Steamworks initialized with SteamID {SteamId}!");

                        HookSteamEvents();

                        _isInitialized = true;
                        break;
                    }
                case (ulong)MessageTypes.GetUsername:
                    {
                        string username = dataReader.GetString();
                        PlayerIdManager.SetUsername(username);
                    }
                    break;
                case (ulong)MessageTypes.OnDisconnected:
                    ulong longId = dataReader.GetULong();
                    if (PlayerIdManager.HasPlayerId(longId))
                    {
                        // Update the mod so it knows this user has left
                        InternalServerHelpers.OnUserLeave(longId);

                        // Send disconnect notif to everyone
                        ConnectionSender.SendDisconnect(longId);
                    }
                    break;
                case (ulong)MessageTypes.OnMessage:
                    {
                        byte[] data = dataReader.GetBytesWithLength();
                        ProxySocketHandler.OnSocketMessageReceived(data, true);
                        break;
                    }
                case (ulong)MessageTypes.OnConnectionDisconnected:
                    NetworkHelper.Disconnect();
                    break;
                case (ulong)MessageTypes.OnConnectionMessage:
                    {
                        byte[] data = dataReader.GetBytesWithLength();
                        ProxySocketHandler.OnSocketMessageReceived(data, false);
                        break;
                    }
                case (ulong)MessageTypes.JoinServer:
                    {
                        ulong serverId = dataReader.GetULong();
                        JoinServer(new SteamId()
                        {
                            Value = serverId
                        });
                    }
                    break;
                case (ulong)MessageTypes.StartServer:
                    {
                        _isServerActive = true;
                        _isConnectionActive = true;

                        // Call server setup
                        InternalServerHelpers.OnStartServer();

                        OnUpdateSteamLobby();
                        OnUpdateRichPresence();
                        break;
                    }
                case (ulong)MessageTypes.LobbyIds:
                case (ulong)MessageTypes.LobbyMetadata:
                    {
                        _lobbyManager.HandleLobbyMessage((MessageTypes)id, dataReader);
                        break;
                    }
                case (ulong)MessageTypes.DecompressVoice:
                    {
                        ulong playerLong = dataReader.GetULong();
                        byte[] data = dataReader.GetBytesWithLength();
                        var identifier = ProxyVoiceIdentifier.GetVoiceIdentifier(PlayerIdManager.GetPlayerId(playerLong));
                        identifier.OnDecompressedVoiceBytesReceived(data);
                        break;
                    }
            }

            dataReader.Recycle();
        }

        internal override void OnLateInitializeLayer()
        {
        }

        internal override void OnCleanupLayer()
        {
            client.Stop();
            
            Disconnect();

            UnHookSteamEvents();
        }

        internal override void OnUpdateLayer()
        {
            client.PollEvents();
        }

        internal static NetDataWriter NewWriter(MessageTypes type)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((byte)type);
            return writer;
        }

        internal void SendToProxyServer(NetDataWriter writer)
        {
            if (serverConnection == null)
            {
                FusionLogger.Warn("Attempting to send data to a null server peer! Is the proxy active?");
                return;
            }

            serverConnection.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        internal void SendToProxyServer(MessageTypes type)
        {
            if (serverConnection == null)
            {
                FusionLogger.Warn("Attempting to send data to a null server peer! Is the proxy active?");
                return;
            }

            NetDataWriter writer = NewWriter(type);
            serverConnection.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        internal override void OnVoiceChatUpdate()
        {
            if (NetworkInfo.HasServer)
                ProxyVoiceIdentifier.OnUpdate();

            /*if (NetworkInfo.HasServer)
            {
                bool voiceEnabled = FusionPreferences.ActiveServerSettings.VoicechatEnabled.GetValue() && !FusionPreferences.ClientSettings.Muted && !FusionPreferences.ClientSettings.Deafened;

                // Update voice record
                if (SteamUser.VoiceRecord != voiceEnabled)
                    SteamUser.VoiceRecord = voiceEnabled;

                // Read voice data
                if (voiceEnabled && SteamUser.HasVoiceData)
                {
                    // yea yea creates a new array every call.
                    // if you find this and are bothered to replace it with the mem stream version then go ahead
                    byte[] voiceData = SteamUser.ReadVoiceDataBytes();

                    PlayerSender.SendPlayerVoiceChat(voiceData);
                }

                // Update identifiers
                ProxyVoiceIdentifier.OnUpdate();
            }
            else
            {
                // Disable voice recording
                if (SteamUser.VoiceRecord)
                    SteamUser.VoiceRecord = false;
            }*/
        }

        internal override void OnVoiceBytesReceived(PlayerId id, byte[] bytes)
        {
            // If we are deafened, no need to deal with voice chat
            bool isDeafened = !FusionPreferences.ActiveServerSettings.VoicechatEnabled.GetValue() || FusionPreferences.ClientSettings.Deafened;
            if (isDeafened)
                return;

            var identifier = ProxyVoiceIdentifier.GetVoiceIdentifier(id);

            if (identifier != null)
            {
                identifier.OnVoiceBytesReceived(bytes);
            }
        }

        internal override void BroadcastMessage(NetworkChannel channel, FusionMessage message)
        {
            if (IsServer)
            {
                ProxySocketHandler.BroadcastToClients(channel, message);
            }
            else
            {
                ProxySocketHandler.BroadcastToServer(channel, message);
            }
        }

        internal override void SendToServer(NetworkChannel channel, FusionMessage message)
        {
            ProxySocketHandler.BroadcastToServer(channel, message);
        }

        internal override void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message)
        {
            var id = PlayerIdManager.GetPlayerId(userId);
            if (id != null)
                SendFromServer(id.LongId, channel, message);
        }

        internal override void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message)
        {
            if (IsServer)
            {
                MessageTypes type = channel == NetworkChannel.Unreliable ? MessageTypes.UnreliableSendFromServer : MessageTypes.ReliableSendFromServer;
                NetDataWriter writer = NewWriter(type);
                writer.Put(userId);
                byte[] data = message.ToByteArray();
                writer.PutBytesWithLength(data);
                SendToProxyServer(writer);
            }
        }

        internal override void StartServer()
        {
            SendToProxyServer(MessageTypes.StartServer);
        }

        public void JoinServer(SteamId serverId)
        {
            // Leave existing server
            if (_isConnectionActive || _isServerActive)
                Disconnect();

            NetDataWriter writer = NewWriter(MessageTypes.JoinServer);
            writer.Put(serverId);
            SendToProxyServer(writer);

            _isServerActive = false;
            _isConnectionActive = true;

            ConnectionSender.SendConnectionRequest();

            OnUpdateSteamLobby();
            OnUpdateRichPresence();
        }

        internal override void Disconnect(string reason = "")
        {
            // Make sure we are currently in a server
            if (!_isServerActive && !_isConnectionActive)
                return;

            try
            {
                SendToProxyServer(MessageTypes.Disconnect);
            }
            catch
            {
                FusionLogger.Log("Error closing socket server / connection manager");
            }

            _isServerActive = false;
            _isConnectionActive = false;

            InternalServerHelpers.OnDisconnect(reason);

            OnUpdateSteamLobby();
            OnUpdateRichPresence();
        }

        private void OnUpdateRichPresence()
        {
            string data = _isConnectionActive ? "true" : "null";
            NetDataWriter writer = NewWriter(MessageTypes.UpdateConnectPresence);
            writer.Put(data);
            SendToProxyServer(writer);
        }

        private void HookSteamEvents()
        {
            // Add server hooks
            MultiplayerHooking.OnMainSceneInitialized += OnUpdateSteamLobby;
            MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged += OnUpdateSteamLobby;
            MultiplayerHooking.OnDisconnect += OnDisconnect;
        }

        private void OnPlayerJoin(PlayerId id)
        {
            if (!id.IsSelf)
                ProxyVoiceIdentifier.GetVoiceIdentifier(id);

            OnUpdateSteamLobby();
        }

        private void OnPlayerLeave(PlayerId id)
        {
            ProxyVoiceIdentifier.RemoveVoiceIdentifier(id);

            OnUpdateSteamLobby();
        }

        private void OnDisconnect()
        {
            ProxyVoiceIdentifier.CleanupAll();
        }

        private void UnHookSteamEvents()
        {
            // Remove steam hooks
            //SteamFriends.OnGameRichPresenceJoinRequested -= OnGameRichPresenceJoinRequested;

            // Remove server hooks
            MultiplayerHooking.OnMainSceneInitialized -= OnUpdateSteamLobby;
            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged -= OnUpdateSteamLobby;
            MultiplayerHooking.OnDisconnect -= OnDisconnect;
        }

        private void OnUpdateSteamLobby()
        {
            // Make sure the lobby exists
            if (CurrentLobby == null)
            {
#if DEBUG
                FusionLogger.Warn("Tried updating the steam lobby, but it was null!");
#endif
                return;
            }

            // Write active info about the lobby
            LobbyMetadataHelper.WriteInfo(CurrentLobby);

            // Update bonemenu items
            OnUpdateCreateServerText();
        }

        internal override void OnSetupBoneMenu(MenuCategory category)
        {
            // Create the basic options
            CreateMatchmakingMenu(category);
            BoneMenuCreator.CreateGamemodesMenu(category);
            BoneMenuCreator.CreateSettingsMenu(category);
            BoneMenuCreator.CreateNotificationsMenu(category);
            BoneMenuCreator.CreateBanListMenu(category);

#if DEBUG
            // Debug only (dev tools)
            BoneMenuCreator.CreateDebugMenu(category);
#endif
        }

        // Matchmaking menu
        private MenuCategory _serverInfoCategory;
        private MenuCategory _manualJoiningCategory;
        private MenuCategory _publicLobbiesCategory;
        //private MenuCategory _friendsCategory;

        private void CreateMatchmakingMenu(MenuCategory category)
        {
            // Root category
            var matchmaking = category.CreateCategory("Matchmaking", Color.red);

            // Server making
            _serverInfoCategory = matchmaking.CreateCategory("Server Info", Color.white);
            CreateServerInfoMenu(_serverInfoCategory);

            // Manual joining
            _manualJoiningCategory = matchmaking.CreateCategory("Manual Joining", Color.white);
            CreateManualJoiningMenu(_manualJoiningCategory);

            // Public lobbies list
            _publicLobbiesCategory = matchmaking.CreateCategory("Public Lobbies", Color.white);
            _publicLobbiesCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshPublicLobbies);
            _publicLobbiesCategory.CreateFunctionElement("Select Refresh to load servers!", Color.yellow, null);

            // Steam friends list
            //_friendsCategory = matchmaking.CreateCategory("Steam Friends", Color.white);
            //_friendsCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshFriendLobbies);
            //_friendsCategory.CreateFunctionElement("Select Refresh to load servers!", Color.yellow, null);
        }

        private FunctionElement _createServerElement;

        private void CreateServerInfoMenu(MenuCategory category)
        {
            _createServerElement = category.CreateFunctionElement("Create Server", Color.white, OnClickCreateServer);
            category.CreateFunctionElement("Copy SteamID to Clipboard", Color.white, OnCopySteamID);

            BoneMenuCreator.CreatePlayerListMenu(category);
        }

        private void OnClickCreateServer()
        {
            // Is a server already running? Disconnect
            if (_isConnectionActive)
            {
                Disconnect();
            }
            // Otherwise, start a server
            else
            {
                StartServer();
            }
        }

        private void OnCopySteamID()
        {
            Clipboard.SetText(SteamId.Value.ToString());
        }

        private void OnUpdateCreateServerText()
        {
            if (FusionSceneManager.IsDelayedLoading())
                return;

            if (_isConnectionActive)
                _createServerElement.SetName("Disconnect from Server");
            else
                _createServerElement.SetName("Create Server");
        }

        private FunctionElement _targetServerElement;

        private void CreateManualJoiningMenu(MenuCategory category)
        {
            category.CreateFunctionElement("Join Server", Color.white, OnClickJoinServer);
            _targetServerElement = category.CreateFunctionElement("Server ID:", Color.white, null);
            category.CreateFunctionElement("Paste Server ID from Clipboard", Color.white, OnPasteServerID);
        }

        private void OnClickJoinServer()
        {
            JoinServer(_targetServerId);
        }

        private void OnPasteServerID()
        {
            var text = Clipboard.GetText();
            if (!string.IsNullOrWhiteSpace(text) && ulong.TryParse(text, out var result))
            {
                _targetServerId = result;
                _targetServerElement.SetName($"Server ID: {_targetServerId}");
            }
        }

        private LobbySortMode _publicLobbySortMode = LobbySortMode.LEVEL;
        private bool _isPublicLobbySearching = false;

        private const int _maxLobbiesInOneFrame = 1;
        private const int _lobbyFrameDelay = 10;

        private void Menu_RefreshPublicLobbies()
        {
            // Make sure we arent already searching
            if (_isPublicLobbySearching)
                return;

            // Clear existing lobbies
            _publicLobbiesCategory.Elements.Clear();
            _publicLobbiesCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshPublicLobbies);
            _publicLobbiesCategory.CreateEnumElement("Sort By", Color.white, _publicLobbySortMode, (v) =>
            {
                _publicLobbySortMode = v;
                Menu_RefreshPublicLobbies();
            });

            MelonCoroutines.Start(CoAwaitLobbyListRoutine());
        }

        private bool Internal_CanShowLobby(LobbyMetadataInfo info)
        {
            // Make sure the lobby is actually open
            if (!info.HasServerOpen)
                return false;

            // Decide if this server is too private

            switch (info.Privacy)
            {
                default:
                case ServerPrivacy.LOCKED:
                case ServerPrivacy.PRIVATE:
                    return false;
                case ServerPrivacy.PUBLIC:
                    return true;
                case ServerPrivacy.FRIENDS_ONLY:
                    return IsFriend(info.LobbyId);
            }
        }

        class ProxyNetworkLobby : INetworkLobby
        {
            public LobbyMetadataInfo info;

            public string GetMetadata(string key)
            {
                throw new NotImplementedException();
            }

            public void SetMetadata(string key, string value)
            {
                throw new NotImplementedException();
            }

            public bool TryGetMetadata(string key, out string value)
            {
                throw new NotImplementedException();
            }

            public Action CreateJoinDelegate(LobbyMetadataInfo info)
            {
                if (NetworkInfo.CurrentNetworkLayer is ProxyNetworkLayer proxyLayer) {
                    return () => proxyLayer.JoinServer(info.LobbyId);
                }

                return null;
            }
        }

        private IEnumerator CoAwaitLobbyListRoutine()
        {
            _isPublicLobbySearching = true;
            LobbySortMode sortMode = _publicLobbySortMode;

            // Fetch lobbies
            var task = _lobbyManager.RequestLobbyIds();

            while (!task.IsCompleted)
                yield return null;
;
            var lobbies = task.Result;
            int lobbyCount = 0;

            foreach (var lobby in lobbies)
            {
                // TODO: Make sure this is not us

                var metadataTask = _lobbyManager.RequestLobbyMetadataInfo(lobby);

                while (!metadataTask.IsCompleted)
                    yield return null;


                LobbyMetadataInfo info = metadataTask.Result;

                if (Internal_CanShowLobby(info))
                {
                    // Add to list
                    ProxyNetworkLobby networkLobby = new ProxyNetworkLobby()
                    {
                        info = info
                    };
                    BoneMenuCreator.CreateLobby(_publicLobbiesCategory, info, networkLobby, sortMode);
                }

                lobbyCount++;

                if (lobbyCount >= _maxLobbiesInOneFrame) {
                    lobbyCount = 0;

                    for (var i = 0; i < _lobbyFrameDelay; i++) {
                        yield return null;
                    }
                }

            }

            // Select the updated category
            MenuManager.SelectCategory(_publicLobbiesCategory);

            _isPublicLobbySearching = false;
        }
    }
}