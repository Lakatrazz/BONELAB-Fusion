using System.Collections;

using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Preferences;

//using Steamworks;
//using Steamworks.Data;

using Color = UnityEngine.Color;

using MelonLoader;

using System.Windows.Forms;

using LabFusion.Senders;
using LabFusion.BoneMenu;

using Steamworks;
using FusionHelper.Network;
using LiteNetLib;
using LiteNetLib.Utils;
using BoneLib;
using LabFusion.Voice;
using LabFusion.Voice.Unity;
using LabFusion.SDK.Lobbies;

namespace LabFusion.Network
{
    public abstract class ProxyNetworkLayer : NetworkLayer
    {
        public abstract uint ApplicationID { get; }

        public static ProxyNetworkLayer Instance { get; private set; }

        public override string Title => "Proxy";

        public override bool IsServer => _isServerActive;
        public override bool IsClient => _isConnectionActive;

        public SteamId SteamId;

        private INetworkLobby _currentLobby;
        public override INetworkLobby CurrentLobby => _currentLobby;

        private IVoiceManager _voiceManager;
        public override IVoiceManager VoiceManager => _voiceManager;

        protected bool _isServerActive = false;
        protected bool _isConnectionActive = false;

        protected ulong _targetServerId;

        protected string _targetJoinId;

        protected bool _isInitialized = false;

        private NetManager client;
        private NetPeer serverConnection;
        private ProxyLobbyManager _lobbyManager;

        public override bool CheckSupported()
        {
            return HelperMethods.IsAndroid();
        }

        public override bool CheckValidation()
        {
            return true;
        }

        public override void OnInitializeLayer()
        {
            Instance = this;

            _voiceManager = new UnityVoiceManager();
            _voiceManager.Enable();

            EventBasedNetListener listener = new();
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

                listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
                {
                    FusionLogger.Error("Proxy has disconnected, restarting server discovery!");
                    serverConnection = null;
                    MelonCoroutines.Start(DiscoverServer());
                };

                writer.Put(ApplicationID);
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

        public IEnumerator DiscoverServer()
        {
            int port = FusionPreferences.ClientSettings.ProxyPort.GetValue();
            if (!(port >= 1024 && port <= 65535))
            {
                FusionLogger.Error("Custom port is invalid, using default! (28430)");
                port = 28340;
            }

            float timeElapsed;

            NetDataWriter writer = new();
            writer.Put("FUSION_SERVER_DISCOVERY");

            while (serverConnection == null)
            {
                timeElapsed = 0;
                client.SendBroadcast(writer, port);

                while (timeElapsed < 5)
                {
                    timeElapsed += TimeUtilities.DeltaTime;
                    yield return null;
                }
            }
        }

        public void EvaluateMessage(NetPeer fromPeer, NetPacketReader dataReader, byte channel, DeliveryMethod deliveryMethod)
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

                        OnUpdateLobby();
                        OnUpdateRichPresence();
                        break;
                    }
                case (ulong)MessageTypes.LobbyIds:
                case (ulong)MessageTypes.LobbyMetadata:
                    {
                        _lobbyManager.HandleLobbyMessage((MessageTypes)id, dataReader);
                        break;
                    }
                case (ulong)MessageTypes.SteamFriends:
                    {
                        FriendIds = dataReader.GetULongArray().ToList();
                        break;
                    }
            }

            dataReader.Recycle();
        }

        public override void OnLateInitializeLayer()
        {
        }

        public override void OnCleanupLayer()
        {
            Disconnect();

            client.Stop();
            serverConnection = null;

            UnHookSteamEvents();

            _voiceManager.Disable();
            _voiceManager = null;
        }

        public override void OnUpdateLayer()
        {
            client.PollEvents();
        }

        internal static NetDataWriter NewWriter(MessageTypes type)
        {
            NetDataWriter writer = new();
            writer.Put((byte)type);
            return writer;
        }

        public void SendToProxyServer(NetDataWriter writer)
        {
            if (serverConnection == null)
            {
                FusionLogger.Warn("Attempting to send data to a null server peer! Is the proxy active?");
                FusionNotifier.Send(new FusionNotification()
                {
                    isMenuItem = false,
                    isPopup = true,
                    popupLength = 4,
                    showTitleOnPopup = true,
                    title = "Connection Failed",
                    message = "Failed to send data to the proxy, is FusionHelper running on your computer?",
                    type = NotificationType.ERROR
                });
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

        public static List<ulong> FriendIds = new();
        public override bool IsFriend(ulong userId)
        {
            if (FriendIds.Contains(userId))
                return true;
            else
                return false;
        }

        public override void BroadcastMessage(NetworkChannel channel, FusionMessage message)
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

        public override void SendToServer(NetworkChannel channel, FusionMessage message)
        {
            ProxySocketHandler.BroadcastToServer(channel, message);
        }

        public override void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message)
        {
            var id = PlayerIdManager.GetPlayerId(userId);
            if (id != null)
                SendFromServer(id.LongId, channel, message);
        }

        public override void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message)
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

        public override void StartServer()
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

            OnUpdateLobby();
            OnUpdateRichPresence();
        }

        public override void Disconnect(string reason = "")
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

            OnUpdateLobby();
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
            MultiplayerHooking.OnMainSceneInitialized += OnUpdateLobby;
            MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged += OnUpdateLobby;
            MultiplayerHooking.OnDisconnect += OnDisconnect;

            _currentLobby = new ProxyNetworkLobby();
        }

        private void OnPlayerJoin(PlayerId id)
        {
            if (!id.IsSelf)
                _voiceManager.GetSpeaker(id);

            OnUpdateLobby();
        }

        private void OnPlayerLeave(PlayerId id)
        {
            _voiceManager.RemoveSpeaker(id);

            OnUpdateLobby();
        }

        private void OnDisconnect()
        {
            _voiceManager.ClearManager();
        }

        private void UnHookSteamEvents()
        {
            // Remove steam hooks
            //SteamFriends.OnGameRichPresenceJoinRequested -= OnGameRichPresenceJoinRequested;

            // Remove server hooks
            MultiplayerHooking.OnMainSceneInitialized -= OnUpdateLobby;
            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged -= OnUpdateLobby;
            MultiplayerHooking.OnDisconnect -= OnDisconnect;
        }

        public override void OnUpdateLobby()
        {
            // Make sure the lobby exists
            if (CurrentLobby == null)
            {
#if DEBUG
                FusionLogger.Warn("Tried updating the proxy lobby, but it was null!");
#endif
                return;
            }

            // Write active info about the lobby
            LobbyMetadataHelper.WriteInfo(CurrentLobby);

            // Update bonemenu items
            OnUpdateCreateServerText();

            // Request Steam Friends
            NetDataWriter writer = NewWriter(MessageTypes.SteamFriends);
            SendToProxyServer(writer);
        }

        public override void OnSetupBoneMenu(MenuCategory category)
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
        private MenuCategory _publicLobbiesCategory;
        private MenuCategory _friendsCategory;

        private void CreateMatchmakingMenu(MenuCategory category)
        {
            // Root category
            var matchmaking = category.CreateCategory("Matchmaking", Color.red);

            // Server making
            _serverInfoCategory = matchmaking.CreateCategory("Server Info", Color.white);
            CreateServerInfoMenu(_serverInfoCategory);

            // Manual joining
            //_manualJoiningCategory = matchmaking.CreateCategory("Manual Joining", Color.white);
            //CreateManualJoiningMenu(_manualJoiningCategory);

            // Public lobbies list
            _publicLobbiesCategory = matchmaking.CreateCategory("Public Lobbies", Color.white);
            _publicLobbiesCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshPublicLobbies);
            _publicLobbiesCategory.CreateFunctionElement("Select Refresh to load servers!", Color.yellow, null);

            // Steam friends list
            _friendsCategory = matchmaking.CreateCategory("Steam Friends", Color.white);
            _friendsCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshFriendLobbies);
            _friendsCategory.CreateFunctionElement("Select Refresh to load servers!", Color.yellow, null);
        }

        private FunctionElement _createServerElement;

        private void CreateServerInfoMenu(MenuCategory category)
        {
            _createServerElement = category.CreateFunctionElement("Create Server", Color.white, OnClickCreateServer);
            category.CreateFunctionElement("Copy SteamID to Clipboard", Color.white, OnCopySteamID);

            BoneMenuCreator.PopulateServerInfo(category);
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

        private LobbySortMode _publicLobbySortMode = LobbySortMode.LEVEL;
        private bool _isPublicLobbySearching = false;

        private void Menu_RefreshPublicLobbies()
        {
            // Make sure we arent already searching
            if (_isPublicLobbySearching)
                return;

            // Clear existing lobbies
            _publicLobbiesCategory.Elements.Clear();
            _publicLobbiesCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshPublicLobbies);

            BoneMenuCreator.CreateFilters(_publicLobbiesCategory);

            _publicLobbiesCategory.CreateEnumElement("Sort By", Color.white, _publicLobbySortMode, (v) =>
            {
                _publicLobbySortMode = v;
                Menu_RefreshPublicLobbies();
            });

            MelonCoroutines.Start(CoAwaitLobbyListRoutine());
        }

        private bool _isFriendLobbySearching = false;
        private void Menu_RefreshFriendLobbies()
        {
            // Make sure we arent searching for lobbies already
            if (_isFriendLobbySearching)
                return;

            // Clear existing lobbies
            _friendsCategory.Elements.Clear();
            _friendsCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshFriendLobbies);

            MelonCoroutines.Start(CoAwaitFriendListRoutine());
        }

        private bool Internal_CanShowLobby(LobbyMetadataInfo info)
        {
            // Make sure the lobby is actually open
            if (!info.HasServerOpen)
                return false;

            // Decide if this server is too private
            return info.Privacy switch
            {
                ServerPrivacy.PUBLIC => true,
                ServerPrivacy.FRIENDS_ONLY => IsFriend(info.LobbyId),
                _ => false,
            };
        }

        private IEnumerator CoAwaitLobbyListRoutine()
        {
            _isPublicLobbySearching = true;
            LobbySortMode sortMode = _publicLobbySortMode;

            // Fetch lobbies
            var task = _lobbyManager.RequestLobbyIds();

            float timeTaken = 0f;

            while (!task.IsCompleted)
            {
                yield return null;
                timeTaken += TimeUtilities.DeltaTime;

                if (timeTaken >= 20f)
                {
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = "Timed Out",
                        showTitleOnPopup = true,
                        message = "Timed out when requesting lobby ids.",
                        isMenuItem = false,
                        isPopup = true,
                    });
                    _isPublicLobbySearching = false;
                    yield break;
                }
            }


            var lobbies = task.Result;

            using (BatchedBoneMenu.Create())
            {
                foreach (var lobby in lobbies)
                {
                    var metadataTask = _lobbyManager.RequestLobbyMetadataInfo(lobby);

                    timeTaken = 0f;

                    while (!metadataTask.IsCompleted)
                    {
                        yield return null;
                        timeTaken += TimeUtilities.DeltaTime;

                        if (timeTaken >= 20f)
                        {
                            FusionNotifier.Send(new FusionNotification()
                            {
                                title = "Timed Out",
                                showTitleOnPopup = true,
                                message = "Timed out when requesting lobby ids.",
                                isMenuItem = false,
                                isPopup = true,
                            });
                            _isPublicLobbySearching = false;
                            yield break;
                        }
                    }

                    LobbyMetadataInfo info = metadataTask.Result;

                    if (Internal_CanShowLobby(info))
                    {
                        // Add to list
                        ProxyNetworkLobby networkLobby = new()
                        {
                            info = info
                        };

                        if (LobbyFilterManager.FilterLobby(networkLobby, info))
                        {
                            BoneMenuCreator.CreateLobby(_publicLobbiesCategory, info, networkLobby, sortMode);
                        }
                    }
                }
            }

            // Select the updated category
            MenuManager.SelectCategory(_publicLobbiesCategory);

            _isPublicLobbySearching = false;
        }

        private IEnumerator CoAwaitFriendListRoutine()
        {
            _isFriendLobbySearching = true;
            LobbySortMode sortMode = LobbySortMode.NONE;

            // Fetch lobbies
            var task = _lobbyManager.RequestLobbyIds();

            float timeTaken = 0f;

            while (!task.IsCompleted)
            {
                yield return null;
                timeTaken += TimeUtilities.DeltaTime;

                if (timeTaken >= 20f)
                {
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = "Timed Out",
                        showTitleOnPopup = true,
                        message = "Timed out when requesting lobby ids.",
                        isMenuItem = false,
                        isPopup = true,
                    });
                    _isFriendLobbySearching = false;
                    yield break;
                }
            }


            var lobbies = task.Result;

            using (BatchedBoneMenu.Create())
            {
                foreach (var lobby in lobbies)
                {
                    var metadataTask = _lobbyManager.RequestLobbyMetadataInfo(lobby);

                    timeTaken = 0f;

                    while (!metadataTask.IsCompleted)
                    {
                        yield return null;
                        timeTaken += TimeUtilities.DeltaTime;

                        if (timeTaken >= 20f)
                        {
                            FusionNotifier.Send(new FusionNotification()
                            {
                                title = "Timed Out",
                                showTitleOnPopup = true,
                                message = "Timed out when requesting lobby ids.",
                                isMenuItem = false,
                                isPopup = true,
                            });
                            _isFriendLobbySearching = false;
                            yield break;
                        }
                    }

                    LobbyMetadataInfo info = metadataTask.Result;

                    if (!IsFriend(info.LobbyId))
                        continue;

                    if (Internal_CanShowLobby(info))
                    {
                        // Add to list
                        ProxyNetworkLobby networkLobby = new()
                        {
                            info = info
                        };

                        BoneMenuCreator.CreateLobby(_publicLobbiesCategory, info, networkLobby, sortMode);
                    }
                }
            }

            // Select the updated category
            MenuManager.SelectCategory(_publicLobbiesCategory);

            _isPublicLobbySearching = false;
        }
    }
}
