using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;

using SLZ.UI.Radial;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(LevelsPanelView), "SelectItem")]
    public class LevelsPanelViewPatches {
        public static bool Prefix(int idx) {
            // Prevent the menu from loading a different level if we aren't the host
            if (NetworkUtilities.HasServer && !NetworkUtilities.IsServer) {
                return false;
            }

            return true;
        }
    }
}
