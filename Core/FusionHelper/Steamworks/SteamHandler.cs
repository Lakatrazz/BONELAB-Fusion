using Steamworks;

namespace FusionHelper.Steamworks
{
    internal class SteamHandler
    {
        const int APP_ID = 250820;

        public static void Init()
        {
            try
            {
                if (!SteamClient.IsValid)
                    SteamClient.Init(APP_ID, true);
                SteamNetworkingUtils.InitRelayNetworkAccess();
                Console.WriteLine("-------------------------------------");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to initialize Steamworks! \n" + e);
            }
        }
    }
}
