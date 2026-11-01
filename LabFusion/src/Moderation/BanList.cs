using LabFusion.Network;

using System.Text.Json.Serialization;

namespace LabFusion.Moderation;

/// <summary>
/// The list of bans for a server.
/// </summary>
[Serializable]
public class BanList
{
    /// <summary>
    /// The list of bans for each platform.
    /// </summary>
    [JsonPropertyName("bans")]
    public Dictionary<string, List<BanInfo>> BansByPlatform { get; set; } = new();

    /// <summary>
    /// Gets a list of bans for a specific platform, or returns a new list if there is none.
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    public List<BanInfo> GetBans(string platform)
    {
        if (BansByPlatform.TryGetValue(platform, out var bans)) 
        {
            return bans;
        }

        bans = new List<BanInfo>();
        BansByPlatform[platform] = bans;
        return bans;
    }

    /// <summary>
    /// Adds a ban to a specific platform given ban info.
    /// <para>If there is already a ban for the same player, the existing ban will be removed and replaced by the new ban.</para>
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="banInfo"></param>
    public void AddBan(string platform, BanInfo banInfo)
    {
        // Remove a ban if it already exists for this platform ID
        RemoveBan(platform, banInfo.PlatformID);

        var bans = GetBans(platform);
        bans.Add(banInfo);
    }

    /// <summary>
    /// Removes a ban by the platform ID on a specific platform.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="platformID"></param>
    public void RemoveBan(string platform, ClientPlatformID platformID)
    {
        if (!BansByPlatform.TryGetValue(platform, out var bans))
        {
            return;
        }

        bans.RemoveAll((info) => info.PlatformID == platformID);
    }

    /// <summary>
    /// Returns true if the platform ID is banned on a specific platform.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public bool IsBanned(string platform, ClientPlatformID platformID)
    {
        if (!BansByPlatform.TryGetValue(platform, out var bans))
        {
            return false;
        }

        return bans.Exists((info) => info.PlatformID == platformID);
    }
}