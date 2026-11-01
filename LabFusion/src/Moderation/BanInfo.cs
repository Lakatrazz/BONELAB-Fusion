using LabFusion.Network;

using System.Text.Json.Serialization;

namespace LabFusion.Moderation;

/// <summary>
/// A ban for a player.
/// </summary>
[Serializable]
public class BanInfo
{
    /// <summary>
    /// The platform ID of the player that is banned.
    /// </summary>
    [JsonPropertyName("player")]
    public ClientPlatformID PlatformID { get; set; } = ClientPlatformID.Empty;

    /// <summary>
    /// The reason the player is banned.
    /// </summary>
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = null;
}
