using LabFusion.Extensions;
using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Representation {
    public static class PlayerRepManager {
        // This should never change, incase other mods rely on it.
        public const string PlayerRepName = "[RigManager (FUSION PlayerRep)]";

        public static readonly List<PlayerRep> PlayerReps = new List<PlayerRep>();
        public static readonly Dictionary<byte, PlayerRep> IDLookup = new Dictionary<byte, PlayerRep>(); 
        public static readonly Dictionary<RigManager, PlayerRep> ManagerLookup = new Dictionary<RigManager, PlayerRep>(new UnityComparer());

        public static bool HasPlayerId(RigManager manager) {
            return ManagerLookup.ContainsKey(manager);
        }

        public static bool TryGetPlayerRep(byte id, out PlayerRep playerRep) {
            return IDLookup.TryGetValue(id, out playerRep);
        }

        public static bool TryGetPlayerRep(RigManager manager, out PlayerRep playerRep) {
            if (manager == null) {
                playerRep = null;
                return false;
            }

            return ManagerLookup.TryGetValue(manager, out playerRep);
        }

        internal static void Internal_InsertPlayerRep(PlayerRep rep) {
            PlayerReps.Add(rep);
            IDLookup.Add(rep.PlayerId.SmallId, rep);
        }

        internal static void Internal_RemovePlayerRep(PlayerRep rep) {
            PlayerReps.Remove(rep);
            IDLookup.Remove(rep.PlayerId.SmallId);
        }

        internal static void Internal_AddRigManager(RigManager manager, PlayerRep rep) {
            ManagerLookup.Add(manager, rep);
        }
    }
}
