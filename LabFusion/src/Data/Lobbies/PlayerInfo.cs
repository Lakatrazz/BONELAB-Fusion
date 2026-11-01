using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Permissions;

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
    public ClientPlatformID PlatformID { get; init; } = ClientPlatformID.Empty;

    /// <summary>
    /// The account username of the player.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; init; } = null;

    /// <summary>
    /// The chosen nickname of the player.
    /// </summary>
    [JsonPropertyName("nickname")]
    public string Nickname { get; init; } = null;

    /// <summary>
    /// The chosen description of the player.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; } = null;

    /// <summary>
    /// The player's permission level in the server.
    /// </summary>
    [JsonPropertyName("permissionLevel")]
    public PermissionLevel PermissionLevel { get; init; } = PermissionLevel.Default;

    /// <summary>
    /// The title of the player's active avatar.
    /// </summary>
    [JsonPropertyName("avatarTitle")]
    public string AvatarTitle { get; init; } = null;
    
    /// <summary>
    /// The mod ID of the player's active avatar, or -1 if invalid.
    /// </summary>
    [JsonPropertyName("avatarModID")]
    public int AvatarModID { get; init; } = -1;

    /// <summary>
    /// Creates PlayerInfo from a platform ID.
    /// <para>This will not have any information about the player except for the ID. A default username will be assigned.</para>
    /// </summary>
    /// <param name="platformID"></param>
    public PlayerInfo(ClientPlatformID platformID)
    {
        PlatformID = platformID;
        Username = $"Unknown ({platformID})";
    }

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
