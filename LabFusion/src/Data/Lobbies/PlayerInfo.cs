using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Representation;

using System.Text.Json.Serialization;

namespace LabFusion.Data;

[Serializable]
public class PlayerInfo
{
    [JsonPropertyName("longId")]
    public ulong LongId { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("permissionLevel")]
    public PermissionLevel PermissionLevel { get; set; }

    [JsonPropertyName("avatarTitle")]
    public string AvatarTitle { get; set; }

    [JsonPropertyName("avatarModId")]
    public int AvatarModId { get; set; } = -1;

    public PlayerInfo() { }

    public PlayerInfo(PlayerID playerId)
    {
        LongId = playerId.PlatformID;

        Username = playerId.Metadata.Username.GetValue();
        Nickname = playerId.Metadata.Nickname.GetValue();
        Description = playerId.Metadata.Description.GetValue();

        playerId.TryGetPermissionLevel(out var level);
        PermissionLevel = level;

        if (NetworkPlayerManager.TryGetPlayer(playerId, out var networkPlayer) && networkPlayer.HasRig)
        {
            var crate = networkPlayer.RigRefs.RigManager.AvatarCrate.Crate;
            AvatarTitle = crate.Title;
        }

        AvatarTitle = playerId.Metadata.AvatarTitle.GetValue();
        AvatarModId = playerId.Metadata.AvatarModID.GetValue();
    }
}
