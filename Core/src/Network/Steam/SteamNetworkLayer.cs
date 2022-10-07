using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Utilities;

using Steamworks;
using Steamworks.Data;

using UnityEngine;

namespace LabFusion.Network
{
    public class SteamNetworkLayer : NetworkLayer {
        public const uint ApplicationID = 1592190;

        public override bool IsServer => _isServerActive;

        public SteamId SteamId;

        public static SteamSocketManager SteamServer;
        public static SteamConnectionManager SteamConnection;

        protected bool _isServerActive = false;
        protected bool _isConnectionActive = false;

        protected string _targetJoinId;

        public override void OnInitializeLayer() {
            SteamAPILoader.OnLoadSteamAPI();

            try {
                SteamClient.Init(ApplicationID, false);
            } 
            catch (Exception e) {
                FusionLogger.Error($"Failed to initialize Steamworks! \n{e}");
            }
        }

        public override void OnLateInitializeLayer() { 
            if (SteamClient.IsLoggedOn) {
                SteamId = SteamClient.SteamId;
                FusionLogger.Log($"Steamworks initialized with SteamID {SteamId}!");

                SteamNetworkingUtils.InitRelayNetworkAccess();
            }
            else {
                FusionLogger.Log("Steamworks failed to initialize!");
            }
        }

        public override void OnCleanupLayer() {
            SteamAPILoader.OnFreeSteamAPI();
        }

        public override void OnUpdateLayer() {
            SteamClient.RunCallbacks();

            try {
                if (SteamServer != null) {
                    SteamServer.Receive();
                }
                if (SteamConnection != null) {
                    SteamConnection.Receive();
                }
            }
            catch {
                FusionLogger.Log("Error receiving data on socket/connection!");
            }
        }

        public override void BroadcastMessage(NetworkChannel channel, FusionMessage message) {
            if (IsServer) {
                SteamSocketHandler.BroadcastToClients(SteamServer, channel, message);
            }
            else {
                SteamSocketHandler.BroadcastToServer(channel, message);
            }
        }

        public void StartServer()
        {
            SteamServer = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);

            // Host needs to connect to own socket server with a ConnectionManager to send/receive messages
            // Relay Socket servers are created/connected to through SteamIds rather than "Normal" Socket Servers which take IP addresses
            SteamConnection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(SteamId);
            _isServerActive = true;
            _isConnectionActive = true;
        }

        public void JoinServer(SteamId serverId)
        {
            FusionLogger.Log("Joining socket server!");
            SteamConnection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(serverId, 0);
            _isServerActive = false;
            _isConnectionActive = true;
        }

        private void Disconnect()
        {
            _isServerActive = false;
            _isConnectionActive = false;
            try
            {
                // Shutdown connections/sockets. I put this in try block because if player 2 is leaving they don't have a socketManager to close, only connection
                SteamConnection.Close();
                SteamServer.Close();
            }
            catch
            {
                FusionLogger.Log("Error closing socket server / connection manager");
            }
        }

        public override void OnGUILayer() {
            var origin = new Vector2(10, 10);
            var size = new Vector2(300, 20);

            GUI.Label(new Rect(origin, size), "SteamID");

            origin.y += 30;

            _targetJoinId = GUI.TextField(new Rect(origin, size), _targetJoinId);

            origin.y += 30;

            if (_isServerActive) {
                if (GUI.Button(new Rect(origin, size), "Stop Server"))
                    Disconnect();
            }
            else if (_isConnectionActive) {
                if (GUI.Button(new Rect(origin, size), "Disconnect"))
                    Disconnect();

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
        }
    }
}
