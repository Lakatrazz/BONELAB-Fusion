using LabFusion.Data;
using LabFusion.Representation;

using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities {
    public static class FusionPlayer {
        public static byte? LastAttacker { get; internal set; }

        internal static void OnMainSceneInitialized() {
            LastAttacker = null;
        }

        /// <summary>
        /// Tries to get the player that we were last attacked by.
        /// </summary>
        /// <returns></returns>
        public static bool TryGetLastAttacker(out PlayerId id) {
            id = null;

            if (!LastAttacker.HasValue)
                return false;

            id = PlayerIdManager.GetPlayerId(LastAttacker.Value);
            return id != null;
        }

        /// <summary>
        /// Checks if the rigmanager is ourselves.
        /// </summary>
        /// <param name="rigManager"></param>
        /// <returns></returns>
        public static bool IsSelf(this RigManager rigManager) {
            return rigManager == RigData.RigReferences.RigManager;
        }
    }
}
