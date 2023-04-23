using BoneLib;
using LabFusion.Extensions;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Senders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public static class MetadataHelper {
        // Default keys
        public const string UsernameKey = "Username";
        public const string NicknameKey = "Nickname";
        public const string LoadingKey = "IsLoading";
        public const string PermissionKey = "PermissionLevel";

        public static bool TryGetPermissionLevel(this PlayerId id, out PermissionLevel level) {
            if (id.TryGetMetadata(PermissionKey, out string rawLevel) && Enum.TryParse(rawLevel, out PermissionLevel newLevel)) {
                level = newLevel;
                return true;
            }

            level = PermissionLevel.DEFAULT;
            return false;
        }

        public static bool TryGetDisplayName(this PlayerId id, out string name) {
            id.TryGetMetadata(UsernameKey, out var username);
            id.TryGetMetadata(NicknameKey, out var nickname);

            // Check validity
            if (FusionMasterList.VerifyPlayer(id.LongId, username) == FusionMasterResult.IMPERSONATOR) {
                username = $"{username} (FAKE)";
            }

            if (FusionMasterList.VerifyPlayer(id.LongId, nickname) == FusionMasterResult.IMPERSONATOR) {
                nickname = $"{nickname} (FAKE)";
            }

            // Convert how the nickname is displayed
            if (!string.IsNullOrWhiteSpace(nickname)) {
                var visibility = FusionPreferences.ClientSettings.NicknameVisibility.GetValue();

                switch (visibility) {
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
            else {
                name = username;
            }

            name = name.LimitLength(PlayerIdManager.MaxNameLength);

            return !string.IsNullOrWhiteSpace(name);
        }
    }
}
