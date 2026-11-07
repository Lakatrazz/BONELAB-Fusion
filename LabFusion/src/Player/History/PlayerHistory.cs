using System.Text.Json.Serialization;

namespace LabFusion.Player;

/// <summary>
/// A history of players that the local game has encountered.
/// </summary>
[Serializable]
public sealed class PlayerHistory
{
    /// <summary>
    /// The list of players by the network layer platform.
    /// </summary>
    [JsonPropertyName("players")]
    public Dictionary<string, PlayerHistoryClientList> PlayersByPlatform { get; set; } = new();

    /// <summary>
    /// Gets a list of player history entries by client for the current platform, or returns a new list if there is none.
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    public PlayerHistoryClientList GetClientList(string platform)
    {
        if (PlayersByPlatform.TryGetValue(platform, out var players))
        {
            return players;
        }

        players = new PlayerHistoryClientList();
        PlayersByPlatform[platform] = players;

        return players;
    }
}
