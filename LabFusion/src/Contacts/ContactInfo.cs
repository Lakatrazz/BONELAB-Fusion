using System.Text.Json.Serialization;

namespace LabFusion.Contacts;

/// <summary>
/// Contact settings that have been set for a player.
/// </summary>
[Serializable]
public readonly record struct ContactInfo
{
    /// <summary>
    /// The volume for the contact.
    /// </summary>
    [JsonPropertyName("volume")]
    public float Volume { get; init; } = 1f;

    /// <summary>
    /// Whether the contact has been muted.
    /// </summary>
    [JsonPropertyName("isMuted")]
    public bool IsMuted { get; init; } = false;

    /// <summary>
    /// Whether the contact is followed.
    /// </summary>
    [JsonPropertyName("isFollowing")]
    public bool IsFollowing { get; init; } = false;

    /// <summary>
    /// Whether the contact is blocked.
    /// </summary>
    [JsonPropertyName("isBlocked")]
    public bool IsBlocked { get; init; } = false;

    public ContactInfo() { }
}
