using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Representation;

using System.Text.Json.Serialization;

namespace LabFusion.Data;

[Serializable]
public class PlayerInfo
{
    [JsonPropertyName("platformID")]
    public ulong PlatformID { get; set; }

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

    [JsonPropertyName("avatarModID")]
    public int AvatarModID { get; set; } = -1;

    public PlayerInfo() { }

    public PlayerInfo(PlayerID playerID)
    {
        PlatformID = playerID.PlatformID;

        Username = playerID.Metadata.Username.GetValueOrEmpty();
        Nickname = playerID.Metadata.Nickname.GetValueOrEmpty();
        Description = playerID.Metadata.Description.GetValueOrEmpty();

        playerID.TryGetPermissionLevel(out var level);
        PermissionLevel = level;

        if (NetworkPlayerManager.TryGetPlayer(playerID, out var networkPlayer) && networkPlayer.NetworkRig.HasRig)
        {
            var crate = networkPlayer.NetworkRig.RigRefs.RigManager.AvatarCrate.Crate;
            AvatarTitle = crate.Title;
        }

        AvatarTitle = playerID.Metadata.AvatarTitle.GetValue();
        AvatarModID = playerID.Metadata.AvatarModID.GetValue();
    }
}
