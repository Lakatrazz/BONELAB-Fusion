using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Utilities;
using SLZ.Rig;
using Steamworks;
using Steamworks.Data;

using UnityEngine;

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

        protected string _targetJoinId;

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
            }
            else {
                FusionLogger.Log("Steamworks failed to initialize!");
            }
        }

        internal override void OnCleanupLayer() {
            Disconnect();

            SteamAPILoader.OnFreeSteamAPI();
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
            if (IsServer && SteamSocket.ConnectedSteamIds.ContainsKey(userId)) {
                SteamSocket.SendToClient(SteamSocket.ConnectedSteamIds[userId], channel, message);
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

            // Go ahead and fill in our own id
            var id = new PlayerId(SteamId, 0, PlayerIdManager.LocalUsername);
            id.Insert();
            PlayerIdManager.ApplyLocalId();
        }

        public void JoinServer(SteamId serverId)
        {
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
        }

        internal override void Disconnect()
        {
            try {
                // Shutdown connections/sockets. I put this in try block because if player 2 is leaving they don't have a socketManager to close, only connection
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
        }

        internal override void OnGUILayer() {
            var origin = new Vector2(10, 10);
            var size = new Vector2(300, 20);

            GUI.Label(new Rect(origin, size), "SteamID");

            origin.y += 30;

            _targetJoinId = GUI.TextField(new Rect(origin, size), _targetJoinId);

            origin.y += 30;

            if (_isServerActive) {
                if (GUI.Button(new Rect(origin, size), "Stop Server"))
                    NetworkHelper.Disconnect();
            }
            else if (_isConnectionActive) {
                if (GUI.Button(new Rect(origin, size), "Disconnect"))
                    NetworkHelper.Disconnect();

                origin.y += 30;

                if (GUI.Button(new Rect(origin, size), "Send Hi")) {
                    using (FusionWriter writer = FusionWriter.Create())
                    {
                        writer.Write("Hi! This is my test message!");
                        
                        using (FusionMessage message = FusionMessage.Create(NativeMessageTag.Unknown, writer)) {
                            BroadcastMessage(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
            else {
                if (GUI.Button(new Rect(origin, size), "Start Server"))
                    StartServer();

                origin.y += 30;

                if (GUI.Button(new Rect(origin, size), "Join Server"))
                    JoinServer(ulong.Parse(_targetJoinId));
            }

            origin.y += 30;

            if (GUI.Button(new Rect(origin, size), "Spawn Player Rep")) {
                PlayerRepUtilities.CreateNewRig(OnRigCreated);
            }
        }

        internal void OnRigCreated(RigManager rig) {
            rig.Teleport(RigData.RigReferences.RigManager.physicsRig.feet.transform.position, true);
        }
    }
}
