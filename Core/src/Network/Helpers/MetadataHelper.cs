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

        public static bool TryGetDisplayName(this PlayerId id, out string name) {
            id.TryGetMetadata(UsernameKey, out var username);
            id.TryGetMetadata(NicknameKey, out var nickname);

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

            return !string.IsNullOrWhiteSpace(name);
        }
    }
}
