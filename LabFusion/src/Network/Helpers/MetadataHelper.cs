using LabFusion.Extensions;
using LabFusion.Preferences.Client;
using LabFusion.Representation;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Safety;

namespace LabFusion.Network;

public static class MetadataHelper
{
    public static bool TryGetPermissionLevel(this PlayerID id, out PermissionLevel level)
    {
        var rawLevel = id.Metadata.PermissionLevel.GetValue();

        if (Enum.TryParse(rawLevel, out PermissionLevel newLevel))
        {
            level = newLevel;
            return true;
        }

        level = PermissionLevel.DEFAULT;
        return false;
    }

    public static bool TryGetDisplayName(this PlayerID id, out string name)
    {
        var username = id.Metadata.Username.GetValue();
        var nickname = id.Metadata.Nickname.GetValue();

        username = TextFilter.Filter(username);
        nickname = TextFilter.Filter(nickname);

        // Check validity
        if (FusionMasterList.VerifyPlayer(id.PlatformID, username) == FusionMasterResult.IMPERSONATOR)
        {
            username = $"{username} (FAKE)";
        }

        if (FusionMasterList.VerifyPlayer(id.PlatformID, nickname) == FusionMasterResult.IMPERSONATOR)
        {
            nickname = $"{nickname} (FAKE)";
        }

        // Convert how the nickname is displayed
        if (!string.IsNullOrWhiteSpace(nickname))
        {
            var visibility = ClientSettings.NicknameVisibility.Value;

            switch (visibility)
            {
                default:
                case NicknameVisibility.SHOW_WITH_PREFIX:
                    name = $"~{nickname}";
                    break;
                case NicknameVisibility.SHOW:
                    name = nickname;
                    break;
                case NicknameVisibility.HIDE:
                    name = username;
                    break;
            }
        }
        else
        {
            name = username;
        }

        name = name.LimitLength(PlayerIDManager.MaxNameLength);

        return !string.IsNullOrWhiteSpace(name);
    }
}