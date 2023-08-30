using FusionHelper.Network;
using LiteNetLib.Utils;
using System.Runtime.InteropServices;
using Steamworks;
using System.Security.Cryptography;

namespace FusionHelper.Steamworks
{
    internal static class SteamHandler
    {
        const int RECEIVE_BUFFER_SIZE = 32;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static SteamSocketManager SocketManager { get; private set; }
        public static SteamConnectionManager ConnectionManager { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static bool IsServer;
        private static bool IsInited;

        private static CSteamID? _localLobby;

        // C API things
        private static CallResult<LobbyCreated_t> _createLobbyCallResult;
        private static Callback<GameRichPresenceJoinRequested_t> _gameRichPresJoinRequestedCallback;

        public static void Init(int appId)
        {
#if !PLATFORM_MAC
            try
            {
                string directory = Directory.GetCurrentDirectory();
                File.WriteAllText(Path.Combine(directory, "steam_appid.txt"), appId.ToString());
            }
            catch
            {
                Console.WriteLine("Failed to write the Steam app id to disk, defaulting to SteamVR. Please make sure your in-game settings match with this.");
            }
#endif

            try
            {
                SteamAPI.Init();
                SteamNetworkingUtils.InitRelayNetworkAccess();
                _gameRichPresJoinRequestedCallback = Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequested);
                ConnectionManager = new SteamConnectionManager();
                SocketManager = new SteamSocketManager();

                IsInited = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to initialize Steamworks! \n" + e);
            }

            AwaitLobbyCreation();
        }

        public static void Tick()
        {
            if (IsInited)
            {
                SteamAPI.RunCallbacks();
            }

            try
            {
                SocketManager?.Receive(RECEIVE_BUFFER_SIZE);
                ConnectionManager?.Receive(RECEIVE_BUFFER_SIZE);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed when receiving data on Socket and Connection: {0}", e);
            }
        }

        private static void AwaitLobbyCreation()
        {
            var lobbyTask = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeInvisible, 100);
            _createLobbyCallResult = CallResult<LobbyCreated_t>.Create((a, b) =>
            {
                _localLobby = new CSteamID(a.m_ulSteamIDLobby);
            });
            _createLobbyCallResult.Set(lobbyTask);
        }

        public static void ConnectRelay(ulong serverId)
        {
            SteamNetworkingIdentity id = default;
            id.SetSteamID64(serverId);
            ConnectionManager.Connection = SteamNetworkingSockets.ConnectP2P(ref id, 0, 0, Array.Empty<SteamNetworkingConfigValue_t>());
            IsServer = false;
        }

        public static void CreateRelay()
        {
            SocketManager.Socket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, Array.Empty<SteamNetworkingConfigValue_t>());

            // Host needs to connect to own socket server with a ConnectionManager to send/receive messages
            // Relay Socket servers are created/connected to through SteamIds rather than "Normal" Socket Servers which take IP addresses
            SteamNetworkingIdentity id = default;
            id.SetSteamID64(SteamUser.GetSteamID().m_SteamID);
            ConnectionManager.Connection = SteamNetworkingSockets.ConnectP2P(ref id, 0, 0, Array.Empty<SteamNetworkingConfigValue_t>());

            IsServer = true;
        }

        private static void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t pCallback)
        {
            // Forward this to joining a server from the friend
            NetDataWriter writer = NetworkHandler.NewWriter(MessageTypes.JoinServer);
            writer.Put(pCallback.m_steamIDFriend.m_SteamID);
            NetworkHandler.SendToClient(writer);
        }

        public static void SendToClient(HSteamNetConnection connection, byte[] message, bool reliable)
        {
            SendType sendType = reliable ? SendType.Reliable : SendType.Unreliable;

            // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
            int sizeOfMessage = message.Length;
            IntPtr intPtrMessage = Marshal.AllocHGlobal(sizeOfMessage);
            Marshal.Copy(message, 0, intPtrMessage, sizeOfMessage);

            SteamNetworkingSockets.SendMessageToConnection(connection, intPtrMessage, (uint)sizeOfMessage, (int)sendType, out long _);

            Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
        }

        public static void SetMetadata(string key, string value)
        {
            if (_localLobby == null)
            {
                Console.WriteLine("Attempting to update null lobby.");
                return;
            }

            SteamMatchmaking.SetLobbyData(_localLobby.Value, key, value);
        }

        public static byte[] DecompressVoice(byte[] from)
        {
            // TODO: does not handle if decompressed is over buffer size
            unsafe
            {
                var to = new byte[1024 * 64];
                uint szWritten = 0;

                if (SteamUser.DecompressVoice(from, (uint)from.Length, to, (uint)to.Length, out szWritten, SteamUser.GetVoiceOptimalSampleRate()) != EVoiceResult.k_EVoiceResultOK)
                    return Array.Empty<byte>();

                if (szWritten == 0)
                    return Array.Empty<byte>();

                Array.Resize(ref to, (int)szWritten);

                return to;
            }
        }

        public static bool CheckSteamRunning()
        {
            var procs = System.Diagnostics.Process.GetProcesses();
            bool running = procs.Any(p => p.ProcessName == "steam" || p.ProcessName == "steam_osx");

            if (!running)
                Console.WriteLine("\x1b[91mSteam does not seem to be running, you may need to launch it and restart FusionHelper.\x1b[0m");

            return running;
        }

        public static void KillConnection()
        {
            if (ConnectionManager != null)
                SteamNetworkingSockets.CloseConnection(ConnectionManager.Connection, (int)ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_App_Generic, "Connection killed by FusionHelper", false);

            if (SocketManager != null)
            {
                SteamNetworkingSockets.DestroyPollGroup(SocketManager.PollGroup);
                SteamNetworkingSockets.CloseListenSocket(SocketManager.Socket);
            }


            IsServer = false;
        }
    }
}
