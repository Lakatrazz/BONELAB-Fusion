using LabFusion.Network;
using LabFusion.Player;

using System.Text.Json.Serialization;

namespace LabFusion.Data;

[Serializable]
public class PlayerInfo
{
    [JsonPropertyName("username")]
    public string Username { get; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; }

    public PlayerInfo() { }

    public PlayerInfo(PlayerId playerId)
    {
        Username = playerId.Metadata.GetMetadata(MetadataHelper.UsernameKey);
        Nickname = playerId.Metadata.GetMetadata(MetadataHelper.NicknameKey);
    }
}
