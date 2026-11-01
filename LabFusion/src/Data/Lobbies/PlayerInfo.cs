using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Representation;

using System.Text.Json.Serialization;

namespace LabFusion.Data;

/// <summary>
/// A json serializable snapshot of a player.
/// </summary>
[Serializable]
public sealed class PlayerInfo
{
    /// <summary>
    /// The PlatformID of the player.
    /// </summary>
    [JsonPropertyName("platformID")]
    public ClientPlatformID PlatformID { get; init; }

    /// <summary>
    /// The account username of the player.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; init; }

    /// <summary>
    /// The chosen nickname of the player.
    /// </summary>
    [JsonPropertyName("nickname")]
    public string Nickname { get; init; }

    /// <summary>
    /// The chosen description of the player.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; }

    /// <summary>
    /// The player's permission level in the server.
    /// </summary>
    [JsonPropertyName("permissionLevel")]
    public PermissionLevel PermissionLevel { get; init; }

    /// <summary>
    /// The title of the player's active avatar.
    /// </summary>
    [JsonPropertyName("avatarTitle")]
    public string AvatarTitle { get; init; }
    
    /// <summary>
    /// The mod ID of the player's active avatar, or -1 if invalid.
    /// </summary>
    [JsonPropertyName("avatarModID")]
    public int AvatarModID { get; init; } = -1;

    /// <summary>
    /// Creates PlayerInfo from a PlayerID in the current server.
    /// </summary>
    /// <param name="playerID"></param>
    public PlayerInfo(PlayerID playerID)
    {
        PlatformID = playerID.PlatformID;

        Username = playerID.Metadata.Username.GetValueOrEmpty();
        Nickname = playerID.Metadata.Nickname.GetValueOrEmpty();
        Description = playerID.Metadata.Description.GetValueOrEmpty();

        playerID.TryGetPermissionLevel(out var level);
        PermissionLevel = level;

        AvatarTitle = playerID.Metadata.AvatarTitle.GetValue();
        AvatarModID = playerID.Metadata.AvatarModID.GetValue();
    }
}
