using LabFusion.Data;
using LabFusion.Network;

namespace LabFusion.Moderation;

/// <summary>
/// Manages the saving and loading of player bans for the local server.
/// </summary>
public static class BanManager
{
    /// <summary>
    /// The name of the file storing the bans.
    /// </summary>
    public static readonly string FileName = "bans.json";

    /// <summary>
    /// The relative path of the bans to the persistent data folder.
    /// </summary>
    public static string RelativeFilePath => Path.Combine(ResourcePaths.ServerRelativePath, FileName);

    /// <summary>
    /// The actively loaded local ban list.
    /// </summary>
    public static BanList BanList { get; private set; } = new();

    /// <summary>
    /// Reads the ban list from the file, if available.
    /// </summary>
    public static void ReadFile()
    {
        BanList = new();

        var deserializedList = JsonSaver.ReadJsonFromFileWithBackup<BanList>(RelativeFilePath);

        if (deserializedList != null)
        {
            BanList = deserializedList;
        }
    }

    /// <summary>
    /// Writes the current ban list to the file.
    /// </summary>
    public static void WriteFile()
    {
        JsonSaver.WriteJsonToFileWithBackup(FileName, BanList);
    }

    /// <summary>
    /// Bans a player from the server on a specific platform with a given reason. This will not disconnect them from the server, they will only be added to the ban list.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="playerInfo"></param>
    /// <param name="reason"></param>
    public static void Ban(string platform, PlayerInfo playerInfo, string reason) => Ban(platform, new BanInfo()
    {
        Player = playerInfo,
        Reason = reason,
    });

    /// <summary>
    /// Adds a ban to a specific platform with given ban info. This will not disconnect the banned user from the server, they will only be added to the ban list.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="banInfo"></param>
    public static void Ban(string platform, BanInfo banInfo)
    {
        BanList.AddBan(platform, banInfo);

        WriteFile();
    }

    /// <summary>
    /// Unbans a player from the server on a specific platform with a given reason.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="platformID"></param>
    public static void Unban(string platform, ClientPlatformID platformID)
    {
        BanList.RemoveBan(platform, platformID);

        WriteFile();
    }

    /// <summary>
    /// Returns true if the platform ID is banned on a specific platform.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static bool IsBanned(string platform, ClientPlatformID platformID) => BanList.IsBanned(platform, platformID);
}