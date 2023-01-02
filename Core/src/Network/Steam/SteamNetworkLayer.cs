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

using SLZ.Rig;

using Steamworks;
using Steamworks.Data;

using UnityEngine;

using Color = UnityEngine.Color;

using MelonLoader;

using System.Windows.Forms;

namespace LabFusion.Network
{
    public class SteamNetworkLayer : NetworkLayer {
        public const uint ApplicationID = 1592190;

        public const int ReceiveBufferSize = 32;

        public const bool AsyncCallbacks = false;

        internal override bool IsServer => _isServerActive;
        internal override bool IsClient => _isConnectionActive;

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

        internal override string GetUsername(ulong userId) {
            return new Friend(userId).Name;
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

        internal override void SendServerMessage(byte userId, NetworkChannel channel, FusionMessage message) {
            var id = PlayerIdManager.GetPlayerId(userId);
            if (id != null)
                SendServerMessage(id.LongId, channel, message);
        }

        internal override void SendServerMessage(ulong userId, NetworkChannel channel, FusionMessage message) {
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

            UpdateLobbySettings();
            UpdateRichPresence();
        }

        public void JoinServer(SteamId serverId)
        {
            // Leave existing server
            if (_isConnectionActive || _isServerActive)
                Disconnect();

            FusionLogger.Log("Joining socket server!");
            SteamConnection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(serverId, 0);
            
            _isServerActive = false;
            _isConnectionActive = true;

            using (FusionWriter writer = FusionWriter.Create()) {
                using (ConnectionRequestData data = ConnectionRequestData.Create(SteamId.Value, PlayerIdManager.LocalUsername, RigData.GetAvatarBarcode())) {
                    writer.Write(data);

                    using (FusionMessage message = FusionMessage.Create(NativeMessageTag.ConnectionRequest, writer)) {
                        BroadcastMessage(NetworkChannel.Reliable, message);
                    }
                }
            }

            UpdateLobbySettings();
            UpdateRichPresence();
        }

        internal override void Disconnect()
        {
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
            
            InternalServerHelpers.OnDisconnect();

            UpdateLobbySettings();
            UpdateRichPresence();
        }

        private void UpdateRichPresence() {
            if (_isConnectionActive) {
                SteamFriends.SetRichPresence("connect", "true");
            }
            else {
                SteamFriends.SetRichPresence("connect", null);
            }
        }

        private void HookSteamEvents() {
            SteamFriends.OnGameRichPresenceJoinRequested += OnGameRichPresenceJoinRequested;

            // Create a local lobby
            AwaitLobbyCreation();
        }

        private async void AwaitLobbyCreation() {
            var lobbyTask = await SteamMatchmaking.CreateLobbyAsync();
            _localLobby = lobbyTask.Value;
            _localLobby.SetData("LobbyId", SteamId.Value.ToString());
            _localLobby.SetData("LobbyName", PlayerIdManager.LocalUsername);
            _localLobby.SetData("HasServerOpen", "False");
        }

        private void UnHookSteamEvents() {
            SteamFriends.OnGameRichPresenceJoinRequested -= OnGameRichPresenceJoinRequested;

            // Remove the local lobby
            _localLobby.Leave();
        }

        private void OnGameRichPresenceJoinRequested(Friend friend, string value) {
            // Forward this to joining a server from the friend
            JoinServer(friend.Id);
        }

        private void UpdateLobbySettings() {
            if (_isServerActive) {
                _localLobby.SetData("HasServerOpen", "True");
            }
            else {
                _localLobby.SetData("HasServerOpen", "False");
            }

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
            CreateSettingsMenu(category);
        }

        private void CreateSettingsMenu(MenuCategory category) {
            // Root settings
            var settings = category.CreateCategory("Settings", Color.gray);

            // Server settings
            var serverSettings = settings.CreateCategory("Server Settings", Color.white);

            // Client settings
            var clientSettings = settings.CreateCategory("Client Settings", Color.white);
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

            // Steam friends list
            _friendsCategory = matchmaking.CreateCategory("Steam Friends", Color.white);
            _friendsCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshFriendLobbies);
        }

        private FunctionElement _createServerElement;

        private void CreateServerInfoMenu(MenuCategory category) {
            _createServerElement = category.CreateFunctionElement("Create Server", Color.white, OnClickCreateServer);
            category.CreateFunctionElement("Copy SteamID to Clipboard", Color.white, OnCopySteamID);
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

                string isOpen = lobby.GetData("HasServerOpen");

                if (isOpen == "True") {
                    // Add to list
                    Menu_CreateJoinableLobby(_publicLobbiesCategory, lobby);
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

                var lobbyId = ulong.Parse(lobby.GetData("LobbyId"));
                if (!new Friend(lobbyId).IsFriend)
                    continue;

                string isOpen = lobby.GetData("HasServerOpen");

                if (isOpen == "True")
                {
                    // Add to list
                    Menu_CreateJoinableLobby(_friendsCategory, lobby);
                }
            }

            // Select the updated category
            MenuManager.SelectCategory(_friendsCategory);
        }

        private void Menu_CreateJoinableLobby(MenuCategory category, Lobby lobby) {
            var lobbyId = ulong.Parse(lobby.GetData("LobbyId"));
            var lobbyName = lobby.GetData("LobbyName");

            var userString = $"{lobbyName}'s Server";
            
            var lobbyCategory = category.CreateCategory(userString, Color.white);
            lobbyCategory.CreateFunctionElement("Join Server", Color.white, () => {
                JoinServer(lobbyId);
            });
        }
    }
}
