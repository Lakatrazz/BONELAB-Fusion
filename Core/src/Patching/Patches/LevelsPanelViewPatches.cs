using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;

using SLZ.UI;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(LevelsPanelView), "SelectItem")]
    public class LevelsPanelViewPatches {
        public static bool Prefix(int idx) {
            try
            {
                // Prevent the menu from loading a different level if we aren't the host
                if (NetworkInfo.HasServer && !NetworkInfo.IsServer)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("to execute patch LevelsPanelView.SelectItem", e);
#endif
            }

            return true;
        }
    }
}
