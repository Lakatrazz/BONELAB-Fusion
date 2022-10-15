using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;

using SLZ.Marrow.Pool;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(AssetPoolee), "OnSpawn")]
    public class PooleeSpawnPatch {
        public static void Postfix(AssetPoolee __instance, ulong spawnId) {
            try {
                bool isFadeOutVfx = __instance.spawnableCrate && __instance.spawnableCrate.Barcode == SpawnableWarehouseUtilities.FADE_OUT_BARCODE;

                if (isFadeOutVfx && NetworkInfo.HasServer && !NetworkInfo.IsServer)
                {
                    __instance.gameObject.SetActive(false);
                }
            } 
            catch (Exception e) {
#if DEBUG
                FusionLogger.LogException("to execute patch AssetPoolee.OnSpawn", e);
#endif
            }
        }
    }
}
