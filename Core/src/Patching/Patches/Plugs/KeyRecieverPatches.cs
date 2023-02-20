using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Syncables;
using LabFusion.Utilities;

using SLZ.Interaction;

namespace LabFusion.Patching
{
    // they mispelled receiver, nice.
    [HarmonyPatch(typeof(KeyReciever))]
    public static class KeyRecieverPatches {
        public static bool IgnorePatches = false;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(KeyReciever.OnInteractableHostEnter))]
        public static void OnInteractableHostEnter(KeyReciever __instance, InteractableHost host) {
            if (IgnorePatches)
                return;

            // Check if this key is synced
            var key = host.gameObject.GetComponentInChildren<Key>(true);

            if (NetworkInfo.HasServer && key && KeyExtender.Cache.TryGet(key, out var syncable)) {
                // Make sure the key is inserting
                if (__instance._State == KeyReciever._States.HOVERING && __instance._keyHost == host) {
                    // Check if this is static or synced
                    if (KeyRecieverExtender.Cache.TryGet(__instance, out var receiverSyncable) && receiverSyncable.TryGetExtender<KeyRecieverExtender>(out var receiverExtender)) {
                        KeySender.SendPropKeySlot(syncable.GetId(), receiverSyncable.GetId(), receiverExtender.GetIndex(__instance).Value);
                    }
                    else {
                        KeySender.SendStaticKeySlot(syncable.GetId(), __instance.gameObject);
                    }
                }
            }
        }
    }
}
