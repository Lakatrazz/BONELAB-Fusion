using LabFusion.Data;
using LabFusion.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Representation {
    public enum PermissionLevel {
        DEFAULT = 0,
        OPERATOR = 1,
        OWNER = 2,
    }

    public static class FusionPermissions {
        public static void FetchPermissionLevel(ulong longId, out PermissionLevel level, out Color color) {
            level = PermissionLevel.DEFAULT;
            color = Color.white;

            // Get server level permissions
            if (NetworkInfo.IsServer) {
                if (longId == PlayerIdManager.LocalLongId)
                    level = PermissionLevel.OWNER;
                else {
                    foreach (var tuple in PermissionList.PermittedUsers) {
                        if (tuple.Item1 == longId) {
                            level = tuple.Item3;
                        }
                    }
                }
            }
            // Get client side permissions
            else {
                var id = PlayerIdManager.GetPlayerId(longId);

                if (id != null && id.TryGetMetadata(MetadataHelper.PermissionKey, out string rawLevel)) {
                    Enum.TryParse(rawLevel, out level);
                }
            }
        }

        public static void TrySetPermission(ulong longId, string username, PermissionLevel level) {
            // Set in file
            PermissionList.SetPermission(longId, username, level);

            // Set in server
            var playerId = PlayerIdManager.GetPlayerId(longId);

            if (playerId != null && NetworkInfo.IsServer) {
                playerId.TrySetMetadata(MetadataHelper.PermissionKey, level.ToString());
            }
        }

        public static bool HasSufficientPermissions(PermissionLevel level, PermissionLevel requirement) {
            return level >= requirement;
        }

        public static bool HasHigherPermissions(PermissionLevel level, PermissionLevel requirement) {
            return level > requirement;
        }
    }
}
