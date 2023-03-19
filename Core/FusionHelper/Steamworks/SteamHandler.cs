using Steamworks;

namespace FusionHelper.Steamworks
{
    internal static class SteamHandler
    {
        const bool ASYNC_CALLBACKS = true;
        const int APPLICATION_ID = 250820;
        const int RECEIVE_BUFFER_SIZE = 32;

        // TODO: enable hosting servers
        public static SteamSocketManager SocketManager { get; private set; }
        public static SteamConnectionManager ConnectionManager { get; private set; }

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
                if (SocketManager != null)
                {
                    SocketManager.Receive(RECEIVE_BUFFER_SIZE);
                }
                if (ConnectionManager != null)
                {
                    ConnectionManager.Receive(RECEIVE_BUFFER_SIZE);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed when receiving data on Socket and Connection", e);
            }
        }

        public static void ConnectRelay(ulong serverId)
        {
            ConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(serverId, 0);
        }

        public static void KillConnection()
        {
            ConnectionManager?.Close();
            SocketManager?.Close();
        }
    }
}
