﻿using System.Collections;

using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;

using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;

using Steamworks;
using Steamworks.Data;

using Color = UnityEngine.Color;

using MelonLoader;

using System.Windows.Forms;

using LabFusion.Senders;
using LabFusion.BoneMenu;
using LabFusion.SDK.Gamemodes;
using BoneLib;
using LabFusion.Voice;
using LabFusion.Voice.Unity;
using LabFusion.SDK.Lobbies;

namespace LabFusion.Network
{
    public abstract class SteamNetworkLayer : NetworkLayer
    {
        public abstract uint ApplicationID { get; }

        public const int ReceiveBufferSize = 32;

        // AsyncCallbacks are bad!
        // In Unity/Melonloader, they can cause random crashes, especially when making a lot of calls
        public const bool AsyncCallbacks = false;

        public override string Title => "Steam";

        public override bool IsServer => _isServerActive;
        public override bool IsClient => _isConnectionActive;

        private INetworkLobby _currentLobby;
        public override INetworkLobby CurrentLobby => _currentLobby;

        private IVoiceManager _voiceManager = null;
        public override IVoiceManager VoiceManager => _voiceManager;

        public SteamId SteamId;

        public static SteamSocketManager SteamSocket;
        public static SteamConnectionManager SteamConnection;

        protected bool _isServerActive = false;
        protected bool _isConnectionActive = false;

        protected ulong _targetServerId;

        protected string _targetJoinId;

        protected bool _isInitialized = false;

        // A local reference to a lobby
        // This isn't actually used for joining servers, just for matchmaking
        protected Lobby _localLobby;

        public override bool CheckSupported()
        {
            return !HelperMethods.IsAndroid();
        }

        public override bool CheckValidation()
        {
            // Make sure the API actually loaded
            if (!SteamAPILoader.HasSteamAPI)
                return false;

            try
            {
                // Try loading the steam client
                if (!SteamClient.IsValid)
                    SteamClient.Init(ApplicationID, AsyncCallbacks);

                return true;
            }
            catch (Exception e)
            {
                FusionLogger.LogException($"initializing {Title} layer", e);
                return false;
            }
        }

        public override void OnInitializeLayer()
        {
            try
            {
                if (!SteamClient.IsValid)
                    SteamClient.Init(ApplicationID, AsyncCallbacks);
            }
            catch (Exception e)
            {
                FusionLogger.Error($"Failed to initialize Steamworks! \n{e}");
            }

            _voiceManager = new UnityVoiceManager();
            _voiceManager.Enable();
        }

        public override void OnLateInitializeLayer()
        {
            if (SteamClient.IsValid)
            {
                SteamId = SteamClient.SteamId;
                PlayerIdManager.SetLongId(SteamId.Value);
                PlayerIdManager.SetUsername(GetUsername(SteamId.Value));

                FusionLogger.Log($"Steamworks initialized with SteamID {SteamId} and ApplicationID {ApplicationID}!");

                SteamNetworkingUtils.InitRelayNetworkAccess();

                HookSteamEvents();

                _isInitialized = true;
            }
            else
            {
                FusionLogger.Log("Steamworks failed to initialize!");
            }
        }

        public override void OnCleanupLayer()
        {
            Disconnect();

            UnHookSteamEvents();

            _voiceManager.Disable();
            _voiceManager = null;

            SteamAPI.Shutdown();
        }

        public override void OnUpdateLayer()
        {
            // Run callbacks for our client
            if (!AsyncCallbacks)
            {
                SteamClient.RunCallbacks();
            }

            // Receive any needed messages
            try
            {
                SteamSocket?.Receive(ReceiveBufferSize);

                SteamConnection?.Receive(ReceiveBufferSize);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("receiving data on Socket and Connection", e);
            }
        }

        public override string GetUsername(ulong userId)
        {
            return new Friend(userId).Name;
        }

        public override bool IsFriend(ulong userId)
        {
            return userId == PlayerIdManager.LocalLongId || new Friend(userId).IsFriend;
        }

        public override void BroadcastMessage(NetworkChannel channel, FusionMessage message)
        {
            if (IsServer)
            {
                SteamSocketHandler.BroadcastToClients(SteamSocket, channel, message);
            }
            else
            {
                SteamSocketHandler.BroadcastToServer(channel, message);
            }
        }

        public override void SendToServer(NetworkChannel channel, FusionMessage message)
        {
            SteamSocketHandler.BroadcastToServer(channel, message);
        }

        public override void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message)
        {
            var id = PlayerIdManager.GetPlayerId(userId);

            if (id != null)
            {
                SendFromServer(id.LongId, channel, message);
            }
        }

        public override void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message)
        {
            // Make sure this is actually the server
            if (!IsServer)
            {
                return;
            }

            // Get the connection from the userid dictionary
            if (SteamSocket.ConnectedSteamIds.TryGetValue(userId, out var connection))
            {
                SteamSocket.SendToClient(connection, channel, message);
            }
        }

        public override void StartServer()
        {
            SteamSocket = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);

            // Host needs to connect to own socket server with a ConnectionManager to send/receive messages
            // Relay Socket servers are created/connected to through SteamIds rather than "Normal" Socket Servers which take IP addresses
            SteamConnection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(SteamId);
            _isServerActive = true;
            _isConnectionActive = true;

            // Call server setup
            InternalServerHelpers.OnStartServer();

            OnUpdateLobby();
            OnUpdateRichPresence();
        }

        public void JoinServer(SteamId serverId)
        {
            // Leave existing server
            if (_isConnectionActive || _isServerActive)
                Disconnect();

            SteamConnection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(serverId, 0);

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
                SteamConnection?.Close();

                SteamSocket?.Close();
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
            if (_isConnectionActive)
            {
                SteamFriends.SetRichPresence("connect", "true");
            }
            else
            {
                SteamFriends.SetRichPresence("connect", null);
            }
        }

        private void HookSteamEvents()
        {
            // Add steam hooks
            SteamFriends.OnGameRichPresenceJoinRequested += OnGameRichPresenceJoinRequested;

            // Add server hooks
            MultiplayerHooking.OnMainSceneInitialized += OnUpdateLobby;
            GamemodeManager.OnGamemodeChanged += OnGamemodeChanged;
            MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged += OnUpdateLobby;
            MultiplayerHooking.OnDisconnect += OnDisconnect;

            // Create a local lobby
            AwaitLobbyCreation();
        }

        private void OnGamemodeChanged(Gamemode gamemode)
        {
            OnUpdateLobby();
        }

        private void OnPlayerJoin(PlayerId id)
        {
            if (!id.IsOwner)
                VoiceManager.GetSpeaker(id);

            OnUpdateLobby();
        }

        private void OnPlayerLeave(PlayerId id)
        {
            VoiceManager.RemoveSpeaker(id);

            OnUpdateLobby();
        }

        private void OnDisconnect()
        {
            VoiceManager.ClearManager();
        }

        private void UnHookSteamEvents()
        {
            // Remove steam hooks
            SteamFriends.OnGameRichPresenceJoinRequested -= OnGameRichPresenceJoinRequested;

            // Remove server hooks
            MultiplayerHooking.OnMainSceneInitialized -= OnUpdateLobby;
            GamemodeManager.OnGamemodeChanged -= OnGamemodeChanged;
            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged -= OnUpdateLobby;
            MultiplayerHooking.OnDisconnect -= OnDisconnect;

            // Remove the local lobby
            _localLobby.Leave();
        }

        private async void AwaitLobbyCreation()
        {
            var lobbyTask = await SteamMatchmaking.CreateLobbyAsync();

            if (!lobbyTask.HasValue)
            {
#if DEBUG
                FusionLogger.Log("Failed to create a steam lobby!");
#endif
                return;
            }

            _localLobby = lobbyTask.Value;
            _currentLobby = new SteamLobby(_localLobby);
        }

        private void OnGameRichPresenceJoinRequested(Friend friend, string value)
        {
            // Forward this to joining a server from the friend
            JoinServer(friend.Id);
        }

        public override void OnUpdateLobby()
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

        public override void OnSetupBoneMenu(MenuCategory category)
        {
            // Create the basic options
            CreateMatchmakingMenu(category);
            BoneMenuCreator.CreateUniversalMenus(category);
        }

        // Matchmaking menu
        private MenuCategory _serverInfoCategory;
        private MenuCategory _manualJoiningCategory;
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
            _manualJoiningCategory = matchmaking.CreateCategory("Manual Joining", Color.white);
            CreateManualJoiningMenu(_manualJoiningCategory);

            // Public lobbies list
            _publicLobbiesCategory = matchmaking.CreateCategory("Public Lobbies", Color.white);
            _publicLobbiesCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshPublicLobbies);
            _publicLobbiesCategory.CreateEnumElement("Sort By", Color.white, _publicLobbySortMode, (v) =>
            {
                _publicLobbySortMode = v;
                Menu_RefreshPublicLobbies();
            });
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
            if (!Clipboard.ContainsText())
                return;

            var text = Clipboard.GetText();

            if (!string.IsNullOrWhiteSpace(text) && ulong.TryParse(text, out var result))
            {
                _targetServerId = result;
                _targetServerElement.SetName($"Server ID: {_targetServerId}");
            }
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

        private Task<Lobby[]> FetchLobbies()
        {
            var list = SteamMatchmaking.LobbyList;
            list.FilterDistanceWorldwide();
            list.WithMaxResults(int.MaxValue);
            list.WithSlotsAvailable(int.MaxValue);
            list.WithKeyValue(LobbyConstants.HasServerOpenKey, bool.TrueString);
            return list.RequestAsync();
        }

        private IEnumerator CoAwaitLobbyListRoutine()
        {
            _isPublicLobbySearching = true;
            LobbySortMode sortMode = _publicLobbySortMode;

            // Fetch lobbies
            var task = FetchLobbies();

            while (!task.IsCompleted)
                yield return null;

            var lobbies = task.Result;

            using (BatchedBoneMenu.Create())
            {
                foreach (var lobby in lobbies)
                {
                    // Make sure this is not us
                    if (lobby.Owner.IsMe)
                    {
                        continue;
                    }

                    var networkLobby = new SteamLobby(lobby);
                    var info = LobbyMetadataHelper.ReadInfo(networkLobby);

                    if (Internal_CanShowLobby(info) && LobbyFilterManager.FilterLobby(networkLobby, info))
                    {
                        // Add to list
                        BoneMenuCreator.CreateLobby(_publicLobbiesCategory, info, networkLobby, sortMode);
                    }
                }
            }

            // Select the updated category
            MenuManager.SelectCategory(_publicLobbiesCategory);

            _isPublicLobbySearching = false;
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

        private IEnumerator CoAwaitFriendListRoutine()
        {
            _isFriendLobbySearching = true;

            // Fetch lobbies
            var task = FetchLobbies();

            while (!task.IsCompleted)
                yield return null;

            var lobbies = task.Result;

            using (BatchedBoneMenu.Create())
            {
                foreach (var lobby in lobbies)
                {
                    // Make sure this is not us but is also a friend
                    if (lobby.Owner.IsMe)
                        continue;

                    var networkLobby = new SteamLobby(lobby);
                    var lobbyInfo = LobbyMetadataHelper.ReadInfo(networkLobby);

                    if (!IsFriend(lobbyInfo.LobbyId))
                        continue;

                    if (Internal_CanShowLobby(lobbyInfo))
                    {
                        // Add to list
                        BoneMenuCreator.CreateLobby(_friendsCategory, lobbyInfo, networkLobby);
                    }
                }
            }

            // Select the updated category
            MenuManager.SelectCategory(_friendsCategory);

            _isFriendLobbySearching = false;
        }
    }
}
