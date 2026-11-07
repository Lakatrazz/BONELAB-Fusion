using System.Text.Json.Serialization;

namespace LabFusion.Player;

/// <summary>
/// A list of players that the local game has encountered by their client platform ID.
/// </summary>
[Serializable]
public sealed class PlayerHistoryClientList
{
    [JsonPropertyName("clients")]
    /// <summary>
    /// The list of player history entries by client platform IDs.
    /// </summary>
    public Dictionary<string, PlayerHistoryEntry> EntriesByClient { get; set; } = new();
}
