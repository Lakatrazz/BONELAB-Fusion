using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Data;

using SLZ.Marrow.Pool;
using SLZ.Props.Weapons;

using UnityEngine;
using LabFusion.Syncables;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(SpawnGun), nameof(SpawnGun.OnFire))]
    public class SpawnGunOnFirePatch {
        public static void Postfix(SpawnGun __instance) {
            if (NetworkInfo.HasServer && !NetworkInfo.IsServer) {
                if (__instance._selectedMode == UtilityModes.SPAWNER && __instance._selectedCrate != null) {
                    var crate = __instance._selectedCrate;
                    PooleeUtilities.RequestSpawn(crate.Barcode, new SerializedTransform(__instance.placerPreview.transform));
                }
                else if (__instance._selectedMode == UtilityModes.REMOVER && __instance._hitInfo.rigidbody != null) {
                    var hitBody = __instance._hitInfo.rigidbody;

                    var transform = hitBody.transform;
                    AssetPoolee assetPoolee = null;

                    while (!AssetPoolee.Cache.TryGet(transform.gameObject, out var poolee)) {
                        transform = transform.parent;

                        if (transform == null)
                            break;

                        if (poolee != null)
                            assetPoolee = poolee;
                    }

                    if (assetPoolee != null && PropSyncable.Cache.TryGetValue(assetPoolee.gameObject, out var syncable)) {
                        PooleeUtilities.SendDespawn(syncable.GetId());
                    }
                }
            }
        }
    }
}
