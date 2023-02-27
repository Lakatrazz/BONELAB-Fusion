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
using SLZ.Props;

using UnityEngine;

using LabFusion.Syncables;

using SLZ.Props.Weapons;
using SLZ.Interaction;
using LabFusion.Representation;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Gun))]
    public static class GunPatches {
        public static bool IgnorePatches = false;

        [HarmonyPatch(nameof(Gun.Fire))]
        [HarmonyPrefix]
        public static bool Fire(Gun __instance) {
            if (IgnorePatches)
                return true;

             if (NetworkInfo.HasServer && __instance.cartridgeState == Gun.CartridgeStates.UNSPENT && __instance.triggerGrip)
             {
                 var hand = __instance.triggerGrip.GetHand();

                 if (hand == null)
                     return true;

                 if (PlayerRepManager.HasPlayerId(hand.manager))
                     return false;
             }

            return true;
        }

        [HarmonyPatch(nameof(Gun.OnFire))]
        [HarmonyPrefix]
        public static void OnFire(Gun __instance) {
            if (IgnorePatches)
                return;

            try {
                if (NetworkInfo.HasServer && GunExtender.Cache.TryGet(__instance, out var gunSyncable)) {
                    // Make sure this is being grabbed by our main player
                    if (__instance.triggerGrip && __instance.triggerGrip.attachedHands.Find((Il2CppSystem.Predicate<Hand>)((h) => h.manager == RigData.RigReferences.RigManager))) {
                        using (var writer = FusionWriter.Create(GunShotData.Size)) {
                            var ammoCount = __instance._magState != null ? (byte)__instance._magState.AmmoCount : (byte)0;

                            using (var data = GunShotData.Create(PlayerIdManager.LocalSmallId, ammoCount, gunSyncable.Id)) {
                                writer.Write(data);

                                using (var message = FusionMessage.Create(NativeMessageTag.GunShot, writer)) {
                                    MessageSender.SendToServer(NetworkChannel.Reliable, message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                FusionLogger.LogException("patching Gun.OnFire", e);
            }
        }
    }

    [HarmonyPatch(typeof(SpawnGun))]
    public static class SpawnGunPatches {
        public static bool IgnorePatches = false;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SpawnGun.SetPreviewMesh))]
        public static void SetPreviewMesh(SpawnGun __instance) {
            if (IgnorePatches)
                return;

            if (__instance._selectedCrate != null && NetworkInfo.HasServer && SpawnGunExtender.Cache.TryGet(__instance, out var syncable)) {
                using (var writer = FusionWriter.Create(SpawnGunPreviewMeshData.Size))
                {
                    using (var data = SpawnGunPreviewMeshData.Create(PlayerIdManager.LocalSmallId, syncable.GetId(), __instance._selectedCrate.Barcode))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.SpawnGunPreviewMesh, writer))
                        {
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SpawnGun.OnFire))]
        public static void OnFire(SpawnGun __instance) {
            if (NetworkInfo.HasServer && !NetworkInfo.IsServer) {
                if (__instance._selectedMode == UtilityModes.SPAWNER && __instance._selectedCrate != null) {
                    var crate = __instance._selectedCrate;
                    PooleeUtilities.RequestSpawn(crate.Barcode, new SerializedTransform(__instance.placerPreview.transform));
                }
                else if (__instance._selectedMode == UtilityModes.REMOVER && __instance._hitInfo.rigidbody != null) {
                    var hitBody = __instance._hitInfo.rigidbody;
                    AssetPoolee poolee = hitBody.GetComponentInParent<AssetPoolee>();

                    if (poolee != null && PropSyncable.Cache.TryGet(poolee.gameObject, out var syncable)) {
                        PooleeUtilities.SendDespawn(syncable.GetId());
                    }
                }
            }
        }
    }
}
