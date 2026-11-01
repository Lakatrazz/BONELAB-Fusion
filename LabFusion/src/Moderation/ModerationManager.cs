using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Moderation;

/// <summary>
/// Functions for moderating users in the current server.
/// <para>These will only work if a server is currently running locally. For other players performing moderation, permission commands should be used.</para>
/// </summary>
public static class ModerationManager
{
    /// <summary>
    /// Refreshes bans from the file on disk.
    /// </summary>
    public static void RefreshBans() => BanManager.ReadFile();

    /// <summary>
    /// Returns the list of bans for the current platform.
    /// </summary>
    /// <returns></returns>
    public static List<BanInfo> GetBans() => BanManager.BanList.GetBans(NetworkLayerManager.Platform);

    /// <summary>
    /// Returns true if a platform ID is banned on the current platform.
    /// </summary>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static bool IsBanned(ClientPlatformID platformID) => BanManager.IsBanned(NetworkLayerManager.Platform, platformID);

    /// <summary>
    /// Kicks a user from the current server.
    /// </summary>
    /// <param name="platformID"></param>
    public static void Kick(ClientPlatformID platformID) => ServerManager.SendDisconnect(platformID, "Kicked from Server");

    /// <summary>
    /// Bans a user from the current platform.
    /// </summary>
    /// <param name="playerID"></param>
    public static void Ban(PlayerID playerID)
    {
        var platform = NetworkLayerManager.Platform;

        BanManager.Ban(platform, new PlayerInfo(playerID), "Banned");

        ServerManager.SendDisconnect(playerID.PlatformID, "Banned from Server");
    }

    /// <summary>
    /// Unbans a user from the current platform.
    /// </summary>
    /// <param name="platformID"></param>
    public static void Unban(ClientPlatformID platformID) => BanManager.Unban(NetworkLayerManager.Platform, platformID);
}
