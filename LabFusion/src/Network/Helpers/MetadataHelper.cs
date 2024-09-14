using LabFusion.Extensions;
using LabFusion.Preferences.Client;
using LabFusion.Representation;
using LabFusion.Player;
using LabFusion.Senders;

namespace LabFusion.Network;

public static class MetadataHelper
{
    // Default keys
    public const string UsernameKey = "Username";
    public const string NicknameKey = "Nickname";
    public const string LoadingKey = "IsLoading";
    public const string PermissionKey = "PermissionLevel";

    public static bool TryGetPermissionLevel(this PlayerId id, out PermissionLevel level)
    {
        if (id.Metadata.TryGetMetadata(PermissionKey, out string rawLevel) && Enum.TryParse(rawLevel, out PermissionLevel newLevel))
        {
            level = newLevel;
            return true;
        }

        level = PermissionLevel.DEFAULT;
        return false;
    }

    public static bool TryGetDisplayName(this PlayerId id, out string name)
    {
        id.Metadata.TryGetMetadata(UsernameKey, out var username);
        id.Metadata.TryGetMetadata(NicknameKey, out var nickname);

        // Check validity
        if (FusionMasterList.VerifyPlayer(id.LongId, username) == FusionMasterResult.IMPERSONATOR)
        {
            username = $"{username} (FAKE)";
        }

        if (FusionMasterList.VerifyPlayer(id.LongId, nickname) == FusionMasterResult.IMPERSONATOR)
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

        name = name.LimitLength(PlayerIdManager.MaxNameLength);

        return !string.IsNullOrWhiteSpace(name);
    }
}