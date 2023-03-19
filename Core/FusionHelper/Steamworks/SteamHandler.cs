using Steamworks;
using Steamworks.Data;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace FusionHelper.Steamworks
{
    internal static class SteamHandler
    {
        const bool ASYNC_CALLBACKS = true;
        const int APPLICATION_ID = 250820;
        const int RECEIVE_BUFFER_SIZE = 32;

        // TODO: enable hosting servers
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static SteamSocketManager SocketManager { get; private set; }
        public static SteamConnectionManager ConnectionManager { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static void Init()
        {
            try
            {
                if (!SteamClient.IsValid)
                    SteamClient.Init(APPLICATION_ID, ASYNC_CALLBACKS);
                SteamNetworkingUtils.InitRelayNetworkAccess();
                Console.WriteLine("-------------------------------------");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to initialize Steamworks! \n" + e);
            }
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

        public static void KillConnection()
        {
            ConnectionManager?.Close();
            SocketManager?.Close();
        }
    }
}
