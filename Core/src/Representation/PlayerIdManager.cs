using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Network;

namespace LabFusion.Representation
{
    public static class PlayerIdManager {
        public static readonly List<PlayerId> PlayerIds = new List<PlayerId>();

        public static string LocalUsername { get; private set; } = "[unknown]";
        public static ulong LocalLongId { get; private set; }
        public static byte LocalSmallId => LocalId.SmallId;
        public static PlayerId LocalId { get; private set; }

        public static byte? GetUnusedPlayerId() {
            for (byte i = 0; i < 255; i++) {
                if (GetPlayerId(i) == null)
                    return i;
            }
            return null;
        }

        public static PlayerId GetPlayerId(byte smallId) {
            return PlayerIds.FirstOrDefault(x => x.SmallId == smallId);
        }

        public static PlayerId GetPlayerId(ulong longId) {
            return PlayerIds.FirstOrDefault(x => x.LongId == longId);
        }

        internal static void ApplyLocalId() {
            var id = GetPlayerId(LocalLongId);
            if (id != null)
                LocalId = id;
            else
                LocalId = null;
        }

        internal static void RemoveLocalId() {
            LocalId = null;
        }

        internal static void SetLongId(ulong longId) {
            LocalLongId = longId;
        }

        internal static void SetUsername(string username) {
            LocalUsername = username;
        }
    }
}
