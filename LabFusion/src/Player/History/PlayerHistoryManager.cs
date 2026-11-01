using LabFusion.Data;
using LabFusion.Network;

namespace LabFusion.Player;

/// <summary>
/// Manages the saving and loading of players that the local game has encountered.
/// </summary>
public static class PlayerHistoryManager
{
    /// <summary>
    /// The name of the file storing the player history.
    /// </summary>
    public static readonly string FileName = "player_history.json";

    /// <summary>
    /// The relative path of the player history to the persistent data folder.
    /// </summary>
    public static string RelativeFilePath => FileName; // Just the file name, player history is shared between client and server logic

    /// <summary>
    /// The actively loaded player history.
    /// </summary>
    public static PlayerHistory PlayerHistory { get; private set; } = new();

    /// <summary>
    /// Reads the player history from the file, if available.
    /// </summary>
    public static void ReadFile()
    {
        PlayerHistory = new();

        var deserializedList = JsonSaver.ReadJsonFromFileWithBackup<PlayerHistory>(RelativeFilePath);

        if (deserializedList != null)
        {
            PlayerHistory = deserializedList;
        }
    }

    /// <summary>
    /// Writes the current player history to the file.
    /// </summary>
    public static void WriteFile()
    {
        JsonSaver.WriteJsonToFileWithBackup(FileName, PlayerHistory);
    }

    /// <summary>
    /// Returns a history entry for an encountered player on a given platform, or null if there is none.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static PlayerHistoryEntry GetHistoryEntry(string platform, ClientPlatformID platformID)
    {
        var clientList = PlayerHistory.GetClientList(platform);
        
        if (clientList.EntriesByClient.TryGetValue(platformID, out var entry))
        {
            return entry;
        }

        return null;
    }

    /// <summary>
    /// Returns a history entry for an encountered player on the current platform, or null if there is none.
    /// </summary>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static PlayerHistoryEntry GetHistoryEntry(ClientPlatformID platformID) => GetHistoryEntry(NetworkLayerManager.Platform, platformID);

    /// <summary>
    /// Returns PlayerInfo from player history if available, or default player info if not.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static PlayerInfo GetPlayerInfoOrDefault(string platform, ClientPlatformID platformID)
    {
        var entry = GetHistoryEntry(platform, platformID);
        return entry?.PlayerInfo ?? new PlayerInfo(platformID);
    }

    /// <summary>
    /// Returns PlayerInfo from player history if available, or default player info if not.
    /// </summary>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static PlayerInfo GetPlayerInfoOrDefault(ClientPlatformID platformID) => GetPlayerInfoOrDefault(NetworkLayerManager.Platform, platformID);

    /// <summary>
    /// Updates the history entry for a client that has been recently seen on a given platform.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="platformID"></param>
    public static void UpdateHistoryEntry(string platform, ClientPlatformID platformID)
    {
        var playerHistoryEntry = GetHistoryEntry(platform, platformID) ?? new PlayerHistoryEntry();

        var playerID = PlayerIDManager.GetPlayerID(platformID);

        if (playerID != null)
        {
            playerHistoryEntry.PlayerInfo = new(playerID);
        }

        playerHistoryEntry.LastSeen = DateTime.UtcNow;

        var clientList = PlayerHistory.GetClientList(platform);

        clientList.EntriesByClient[platformID] = playerHistoryEntry;

        WriteFile();
    }

    /// <summary>
    /// Updates the history entry for a client that has been recently seen on the current platform.
    /// </summary>
    /// <param name="platformID"></param>
    public static void UpdateHistoryEntry(ClientPlatformID platformID) => UpdateHistoryEntry(NetworkLayerManager.Platform, platformID);

    internal static void Initialize()
    {
        ReadFile();

        PlayerIDManager.PlayerRegistered += OnPlayerRegistered;
    }

    private static void OnPlayerRegistered(PlayerID playerID)
    {
        UpdateHistoryEntry(playerID.PlatformID);
    }
}
