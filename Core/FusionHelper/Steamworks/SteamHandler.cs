using Steamworks;

namespace FusionHelper.Steamworks
{
    internal class SteamHandler
    {
        const bool ASYNC_CALLBACKS = true;
        const int APPLICATION_ID = 250820;

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

            /*try
            {
                if (SteamSocket != null)
                {
                    SteamSocket.Receive(ReceiveBufferSize);
                }
                if (SteamConnection != null)
                {
                    SteamConnection.Receive(ReceiveBufferSize);
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException("receiving data on Socket and Connection", e);
            }*/
        }
    }
}
