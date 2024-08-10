namespace Steamworks
{
    /// <summary>
    /// Functions for clients to access matchmaking services, favorites, and to operate on game lobbies
    /// </summary>
    public class SteamMatchmakingServers : SteamClientClass<SteamMatchmakingServers>
    {
        internal static ISteamMatchmakingServers Internal => Interface as ISteamMatchmakingServers;

        internal override void InitializeInterface(bool server)
        {
            SetInterface(server, new ISteamMatchmakingServers(server));
        }
    }
}