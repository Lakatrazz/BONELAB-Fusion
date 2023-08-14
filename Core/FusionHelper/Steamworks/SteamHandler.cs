using FusionHelper.Network;
using LiteNetLib.Utils;
using Steamworks;
using Steamworks.Data;
using Steamworks.ServerList;
using System.Runtime.InteropServices;

namespace FusionHelper.Steamworks
{
    internal static class SteamHandler
    {
        const bool ASYNC_CALLBACKS = true;
        const int RECEIVE_BUFFER_SIZE = 32;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static SteamSocketManager SocketManager { get; private set; }
        public static SteamConnectionManager ConnectionManager { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private static Lobby? _localLobby;

        public static void Init(int appId)
        {
#if DEBUG
            Dispatch.OnDebugCallback = (type, str, server) =>
            {
                Console.WriteLine($"[Callback {type} {(server ? "server" : "client")}]");
                Console.WriteLine(str);
                Console.WriteLine($"");
            };

            Dispatch.OnException = (e) =>
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
            };
#endif

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
                if (!SteamClient.IsValid)
                    SteamClient.Init((uint)appId, ASYNC_CALLBACKS);
                SteamNetworkingUtils.InitRelayNetworkAccess();
                SteamFriends.OnGameRichPresenceJoinRequested += OnGameRichPresenceJoinRequested;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to initialize Steamworks! \n" + e);
            }

            AwaitLobbyCreation();
        }

        public static void Tick()
        {
            if (!ASYNC_CALLBACKS)
            {
#pragma warning disable CS0162 // Unreachable code detected
                SteamClient.RunCallbacks();
#pragma warning restore CS0162 // Unreachable code detected
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

        private static async void AwaitLobbyCreation()
        {
            var lobbyTask = await SteamMatchmaking.CreateLobbyAsync();

            if (!lobbyTask.HasValue)
            {
#if DEBUG
                Console.WriteLine("Failed to create a steam lobby!");
#endif
                return;
            }

            _localLobby = lobbyTask.Value;
        }

        public static void ConnectRelay(ulong serverId)
        {
            ConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(serverId, 0);
        }

        public static void CreateRelay()
        {
            SocketManager = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);

            // Host needs to connect to own socket server with a ConnectionManager to send/receive messages
            // Relay Socket servers are created/connected to through SteamIds rather than "Normal" Socket Servers which take IP addresses
            ConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(SteamClient.SteamId);
        }

        private static void OnGameRichPresenceJoinRequested(Friend friend, string value)
        {
            // Forward this to joining a server from the friend
            NetDataWriter writer = NetworkHandler.NewWriter(MessageTypes.JoinServer);
            writer.Put(friend.Id);
            NetworkHandler.SendToClient(writer);
        }

        public static void SendToClient(Connection connection, byte[] message, bool reliable)
        {
            SendType sendType = reliable ? SendType.Reliable : SendType.Unreliable;

            // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
            int sizeOfMessage = message.Length;
            IntPtr intPtrMessage = Marshal.AllocHGlobal(sizeOfMessage);
            Marshal.Copy(message, 0, intPtrMessage, sizeOfMessage);

            connection.SendMessage(intPtrMessage, sizeOfMessage, sendType);

            Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
        }

        public static void SetMetadata(string key, string value)
        {
            if (_localLobby == null)
            {
                Console.WriteLine("Attempting to update null lobby.");
                return;
            }

            _localLobby.Value.SetData(key, value);
        }

        public static byte[] DecompressVoice(byte[] from)
        {
            unsafe
            {
                var to = new byte[1024 * 64];

                uint szWritten = 0;

                fixed (byte* frm = from)
                fixed (byte* dst = to)
                {
                    if (SteamUser.Internal.DecompressVoice((IntPtr)frm, (uint)from.Length, (IntPtr)dst, (uint)to.Length, ref szWritten, SteamUser.SampleRate) != VoiceResult.OK)
                        return Array.Empty<byte>();
                }

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
            ConnectionManager?.Close();
            SocketManager?.Close();
        }
    }
}
