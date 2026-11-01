using LabFusion.Data;

using System.Text.Json.Serialization;

namespace LabFusion.Moderation;

/// <summary>
/// A ban for a player.
/// </summary>
[Serializable]
public class BanInfo
{
    /// <summary>
    /// The player that is banned.
    /// </summary>
    [JsonPropertyName("player")]
    public PlayerInfo Player { get; set; } = null;

    /// <summary>
    /// The reason the player is banned.
    /// </summary>
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = null;
}
