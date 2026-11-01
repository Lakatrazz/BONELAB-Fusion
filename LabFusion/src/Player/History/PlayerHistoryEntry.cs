using LabFusion.Data;

using System.Text.Json.Serialization;

namespace LabFusion.Player;

/// <summary>
/// An entry in the history of players the local game has encountered.
/// </summary>
[Serializable]
public sealed class PlayerHistoryEntry
{
    /// <summary>
    /// The snapshot of the player the last time they were seen.
    /// </summary>
    [JsonPropertyName("playerInfo")]
    public PlayerInfo PlayerInfo { get; set; } = null;
    
    /// <summary>
    /// The last time the local game encountered this player in UTC.
    /// </summary>
    [JsonPropertyName("lastSeen")]
    public DateTime LastSeen { get; set; } = DateTime.MinValue;
}
