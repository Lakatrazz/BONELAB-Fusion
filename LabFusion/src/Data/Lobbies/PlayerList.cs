using LabFusion.Player;

using System.Text.Json.Serialization;

namespace LabFusion.Data;

/// <summary>
/// Represents a JSON serializable list of all players in a lobby.
/// </summary>
[Serializable]
public class PlayerList
{
    [JsonPropertyName("players")]
    public PlayerInfo[] Players { get; set; } = Array.Empty<PlayerInfo>();

    /// <summary>
    /// Writes all of the players in the current lobby to <see cref="Players"/>.
    /// </summary>
    public void WritePlayers()
    {
        // Create player info array from all players
        Players = new PlayerInfo[PlayerIDManager.PlayerCount];
        int index = 0;

        foreach (var player in PlayerIDManager.PlayerIDs)
        {
            Players[index++] = new PlayerInfo(player);
        }
    }
    
    /// <summary>
    /// Checks if the PlayerList contains invalid information, indicating that it may have been tampered with.
    /// </summary>
    /// <returns></returns>
    public bool ValidatePlayers()
    {
        // Validate conflicting fields between each player
        // If any of the players have the same PlatformID, it has likely been tampered with
        var platformIDs = new HashSet<string>(Players.Length);

        foreach (var player in Players)
        {
            var platformID = player.PlatformID;

            // If the PlatformID is invalid, the lobby is probably invalid as well
            if (string.IsNullOrWhiteSpace(platformID) || platformID == "0")
            {
                return false;
            }

            // If the HashSet fails to add the PlatformID, it is a duplicate and therefore has been tampered with
            if (!platformIDs.Add(player.PlatformID))
            {
                return false;
            }
        }

        // All checks passed, the PlayerList is most likely valid
        return true;
    }
}
