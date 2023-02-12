using System;
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

using Steamworks;
using Steamworks.Data;

using UnityEngine;

using Color = UnityEngine.Color;

using MelonLoader;

using System.Windows.Forms;

using LabFusion.Senders;
using LabFusion.BoneMenu;

using System.IO;

using UnhollowerBaseLib;

namespace LabFusion.Network
{
    public class SteamNetworkLayer : NetworkLayer {
        public const uint ApplicationID = 1592190;

        public const int ReceiveBufferSize = 32;

        public const bool AsyncCallbacks = false;

        internal override bool IsServer => _isServerActive;
        internal override bool IsClient => _isConnectionActive;

        private INetworkLobby _currentLobby;
        internal override INetworkLobby CurrentLobby => _currentLobby;

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

        internal override void OnInitializeLayer() {
            SteamAPILoader.OnLoadSteamAPI();

            try {
                SteamClient.Init(ApplicationID, AsyncCallbacks);
            } 
            catch (Exception e) {
                FusionLogger.Error($"Failed to initialize Steamworks! \n{e}");
            }
        }

        internal override void OnLateInitializeLayer() { 
            if (SteamClient.IsLoggedOn) {
                SteamId = SteamClient.SteamId;
                PlayerIdManager.SetLongId(SteamId.Value);
                PlayerIdManager.SetUsername(GetUsername(SteamId.Value));

                FusionLogger.Log($"Steamworks initialized with SteamID {SteamId}!");

                SteamNetworkingUtils.InitRelayNetworkAccess();

                HookSteamEvents();

                _isInitialized = true;
            }
            else {
                FusionLogger.Log("Steamworks failed to initialize!");
            }
        }

        internal override void OnCleanupLayer() {
            Disconnect();

            SteamAPILoader.OnFreeSteamAPI();

            UnHookSteamEvents();
        }

        internal override void OnUpdateLayer() {
            // Run callbacks for our client
            if (!AsyncCallbacks) {
                SteamClient.RunCallbacks();
            }

            // Receive any needed messages
            try {
                if (SteamSocket != null) {
                    SteamSocket.Receive(ReceiveBufferSize);
                }
                if (SteamConnection != null) {
                    SteamConnection.Receive(ReceiveBufferSize);
                }
            }
            catch {
                FusionLogger.Log("Error receiving data on socket/connection!");
            }
        }

        internal override void OnVoiceChatUpdate() {
            if (NetworkInfo.HasServer) {
                bool voiceEnabled = !FusionPreferences.ClientSettings.Muted;

                // Update voice record
                if (SteamUser.VoiceRecord != voiceEnabled)
                    SteamUser.VoiceRecord = voiceEnabled;

                // Read voice data
                if (voiceEnabled && SteamUser.HasVoiceData) {
                    byte[] voiceData = SteamUser.ReadVoiceDataBytes();

                    PlayerSender.SendPlayerVoiceChat(voiceData);
                }
            }
        }

        internal override void OnVoiceBytesReceived(PlayerId id, byte[] bytes) {
            // If we are deafened, no need to deal with voice chat
            bool isDeafened = FusionPreferences.ClientSettings.Deafened;
            if (isDeafened)
                return;

            var identifier = SteamVoiceIdentifier.GetVoiceIdentifier(id);

            if (identifier != null) {
                identifier.OnVoiceBytesReceived(bytes);
            }
        }

        internal override string GetUsername(ulong userId) {
            return new Friend(userId).Name;
        }

        internal override bool IsFriend(ulong userId) {
            return new Friend(userId).IsFriend;
        }

        internal override void BroadcastMessage(NetworkChannel channel, FusionMessage message) {
            if (IsServer) {
                SteamSocketHandler.BroadcastToClients(SteamSocket, channel, message);
            }
            else {
                SteamSocketHandler.BroadcastToServer(channel, message);
            }
        }

        internal override void SendToServer(NetworkChannel channel, FusionMessage message) {
            SteamSocketHandler.BroadcastToServer(channel, message);
        }

        internal override void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message) {
            var id = PlayerIdManager.GetPlayerId(userId);
            if (id != null)
                SendFromServer(id.LongId, channel, message);
        }

        internal override void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message) {
            if (IsServer) {
                if (SteamSocket.ConnectedSteamIds.ContainsKey(userId))
                    SteamSocket.SendToClient(SteamSocket.ConnectedSteamIds[userId], channel, message);
                else if (userId == PlayerIdManager.LocalLongId)
                    SteamSocket.SendToClient(SteamConnection.Connection, channel, message);
            }
        }

        internal override void StartServer()
        {
            SteamSocket = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);
            
            // Host needs to connect to own socket server with a ConnectionManager to send/receive messages
            // Relay Socket servers are created/connected to through SteamIds rather than "Normal" Socket Servers which take IP addresses
            SteamConnection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(SteamId);
            _isServerActive = true;
            _isConnectionActive = true;

            // Call server setup
            InternalServerHelpers.OnStartServer();

            OnUpdateSteamLobby();
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

            OnUpdateSteamLobby();
            OnUpdateRichPresence();
        }

        internal override void Disconnect(string reason = "")
        {
            // Make sure we are currently in a server
            if (!_isServerActive && !_isConnectionActive)
                return;

            try {
                if (SteamConnection != null)
                    SteamConnection.Close();
                
                if (SteamSocket != null)
                    SteamSocket.Close();
            }
            catch {
                FusionLogger.Log("Error closing socket server / connection manager");
            }

            _isServerActive = false;
            _isConnectionActive = false;
            
            InternalServerHelpers.OnDisconnect(reason);

            OnUpdateSteamLobby();
            OnUpdateRichPresence();
        }

        private void OnUpdateRichPresence() {
            if (_isConnectionActive) {
                SteamFriends.SetRichPresence("connect", "true");
            }
            else {
                SteamFriends.SetRichPresence("connect", null);
            }
        }

        private void HookSteamEvents() {
            // Add steam hooks
            SteamFriends.OnGameRichPresenceJoinRequested += OnGameRichPresenceJoinRequested;

            // Add server hooks
            MultiplayerHooking.OnMainSceneInitialized += OnUpdateSteamLobby;
            MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged += OnUpdateSteamLobby;
            MultiplayerHooking.OnDisconnect += OnDisconnect;

            // Create a local lobby
            AwaitLobbyCreation();
        }

        private void OnPlayerJoin(PlayerId id) {
            if (!id.IsSelf)
                SteamVoiceIdentifier.GetVoiceIdentifier(id);

            OnUpdateSteamLobby();
        }

        private void OnPlayerLeave(PlayerId id) {
            SteamVoiceIdentifier.RemoveVoiceIdentifier(id);

            OnUpdateSteamLobby();
        }

        private void OnDisconnect() {
            SteamVoiceIdentifier.CleanupAll();
        }

        private void UnHookSteamEvents() {
            // Remove steam hooks
            SteamFriends.OnGameRichPresenceJoinRequested -= OnGameRichPresenceJoinRequested;
            
            // Remove server hooks
            MultiplayerHooking.OnMainSceneInitialized -= OnUpdateSteamLobby;
            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged -= OnUpdateSteamLobby;
            MultiplayerHooking.OnDisconnect -= OnDisconnect;

            // Remove the local lobby
            _localLobby.Leave();
        }

        private async void AwaitLobbyCreation() {
            var lobbyTask = await SteamMatchmaking.CreateLobbyAsync();
            _localLobby = lobbyTask.Value;
            _currentLobby = new SteamLobby(_localLobby);
        }

        private void OnGameRichPresenceJoinRequested(Friend friend, string value) {
            // Forward this to joining a server from the friend
            JoinServer(friend.Id);
        }

        private void OnUpdateSteamLobby() {
            // Write active info about the lobby
            LobbyMetadataHelper.WriteInfo(CurrentLobby);

            // Update bonemenu items
            OnUpdateCreateServerText();
        }

        internal void OnRigCreated(RigManager rig) {
            rig.Teleport(RigData.RigReferences.RigManager.physicsRig.feet.transform.position, true);
        }

        internal override void OnSetupBoneMenu(MenuCategory category) {
            // This element does nothing
            // Just informs what networking is being used
            category.CreateFunctionElement("Powered by Facepunch.Steamworks", Color.yellow, null);

            // Now for the actual options
            CreateMatchmakingMenu(category);
            BoneMenuCreator.CreateGamemodesMenu(category);
            BoneMenuCreator.CreateSettingsMenu(category);
            BoneMenuCreator.CreateNotificationsMenu(category);
            BoneMenuCreator.CreateBanListMenu(category);

#if DEBUG
            BoneMenuCreator.CreateDebugMenu(category);
#endif
        }

        // Matchmaking menu
        private MenuCategory _serverInfoCategory;
        private MenuCategory _manualJoiningCategory;
        private MenuCategory _publicLobbiesCategory;
        private MenuCategory _friendsCategory;

        private void CreateMatchmakingMenu(MenuCategory category) {
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
            _friendsCategory = matchmaking.CreateCategory("Steam Friends", Color.white);
            _friendsCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshFriendLobbies);
            _friendsCategory.CreateFunctionElement("Select Refresh to load servers!", Color.yellow, null);
        }

        private FunctionElement _createServerElement;

        private void CreateServerInfoMenu(MenuCategory category) {
            _createServerElement = category.CreateFunctionElement("Create Server", Color.white, OnClickCreateServer);
            category.CreateFunctionElement("Copy SteamID to Clipboard", Color.white, OnCopySteamID);

            BoneMenuCreator.CreatePlayerListMenu(category);
        }

        private void OnClickCreateServer() {
            // Is a server already running? Disconnect
            if (_isConnectionActive) {
                Disconnect();
            }
            // Otherwise, start a server
            else {
                StartServer();
            }
        }

        private void OnCopySteamID() {
            Clipboard.SetText(SteamId.Value.ToString());
        }

        private void OnUpdateCreateServerText() {
            if (_isConnectionActive)
                _createServerElement.SetName("Disconnect from Server");
            else
                _createServerElement.SetName("Create Server");
        }

        private FunctionElement _targetServerElement;

        private void CreateManualJoiningMenu(MenuCategory category) {
            category.CreateFunctionElement("Join Server", Color.white, OnClickJoinServer);
            _targetServerElement = category.CreateFunctionElement("Server ID:", Color.white, null);
            category.CreateFunctionElement("Paste Server ID from Clipboard", Color.white, OnPasteServerID);
        }

        private void OnClickJoinServer() {
            JoinServer(_targetServerId);
        }

        private void OnPasteServerID() {
            var text = Clipboard.GetText();
            if (!string.IsNullOrWhiteSpace(text) && ulong.TryParse(text, out var result)) {
                _targetServerId = result;
                _targetServerElement.SetName($"Server ID: {_targetServerId}");
            }
        }

        private void Menu_RefreshPublicLobbies() {
            // Clear existing lobbies
            _publicLobbiesCategory.Elements.Clear();
            _publicLobbiesCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshPublicLobbies);

            MelonCoroutines.Start(CoAwaitLobbyListRoutine());
        }

        private bool Internal_CanShowLobby(LobbyMetadataInfo info) {
            // Make sure the lobby is actually open
            if (!info.HasServerOpen)
                return false;

            // Decide if this server is too private
            switch (info.Privacy) {
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

        private IEnumerator CoAwaitLobbyListRoutine() {
            // Fetch lobbies
            var list = SteamMatchmaking.LobbyList;
            list.FilterDistanceWorldwide();
            var task = list.RequestAsync();

            while (!task.IsCompleted)
                yield return null;

            var lobbies = task.Result;

            foreach (var lobby in lobbies) {
                // Make sure this is not us
                if (lobby.Owner.IsMe)
                    continue;

                var networkLobby = new SteamLobby(lobby);
                var info = LobbyMetadataHelper.ReadInfo(networkLobby);

                if (Internal_CanShowLobby(info)) {
                    // Add to list
                    BoneMenuCreator.CreateLobby(_publicLobbiesCategory, info, networkLobby);
                }
            }

            // Select the updated category
            MenuManager.SelectCategory(_publicLobbiesCategory);
        }

        private void Menu_RefreshFriendLobbies() {
            // Clear existing lobbies
            _friendsCategory.Elements.Clear();
            _friendsCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshFriendLobbies);

            MelonCoroutines.Start(CoAwaitFriendListRoutine());
        }

        private IEnumerator CoAwaitFriendListRoutine()
        {
            // Fetch lobbies
            var list = SteamMatchmaking.LobbyList;
            list.FilterDistanceWorldwide();
            var task = list.RequestAsync();

            while (!task.IsCompleted)
                yield return null;

            var lobbies = task.Result;

            foreach (var lobby in lobbies)
            {
                // Make sure this is not us but is also a friend
                if (lobby.Owner.IsMe)
                    continue;

                var networkLobby = new SteamLobby(lobby);
                var lobbyInfo = LobbyMetadataHelper.ReadInfo(networkLobby);

                if (!IsFriend(lobbyInfo.LobbyId))
                    continue;

                if (Internal_CanShowLobby(lobbyInfo)) {
                    // Add to list
                    BoneMenuCreator.CreateLobby(_friendsCategory, lobbyInfo, networkLobby);
                }
            }

            // Select the updated category
            MenuManager.SelectCategory(_friendsCategory);
        }
    }
}
