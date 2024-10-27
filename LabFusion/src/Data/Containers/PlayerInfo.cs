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

    [JsonPropertyName("permissionLevel")]
    public PermissionLevel PermissionLevel { get; set; }

    [JsonPropertyName("avatarTitle")]
    public string AvatarTitle { get; set; }

    [JsonPropertyName("avatarModId")]
    public int AvatarModId { get; set; } = -1;

    public PlayerInfo() { }

    public PlayerInfo(PlayerId playerId)
    {
        LongId = playerId.LongId;

        Username = playerId.Metadata.GetMetadata(MetadataHelper.UsernameKey);
        Nickname = playerId.Metadata.GetMetadata(MetadataHelper.NicknameKey);

        playerId.TryGetPermissionLevel(out var level);
        PermissionLevel = level;

        if (NetworkPlayerManager.TryGetPlayer(playerId, out var networkPlayer))
        {
            var crate = networkPlayer.RigRefs.RigManager.AvatarCrate.Crate;
            AvatarTitle = crate.Title;
        }
    }
}
