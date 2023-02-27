using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;

using SLZ.Props;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(PropFlashlight))]
    public static class PropFlashlightPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PropFlashlight.SwitchLight))]
        public static bool SwitchLightPrefix(PropFlashlight __instance) {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer && PropFlashlightExtender.Cache.TryGet(__instance, out var syncable) && !syncable.IsOwner())
                return false;

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PropFlashlight.SwitchLight))]
        public static void SwitchLightPostfix(PropFlashlight __instance) {
            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer && PropFlashlightExtender.Cache.TryGet(__instance, out var syncable)) {
                using (var writer = FusionWriter.Create(FlashlightToggleData.Size)) {
                    using (var data = FlashlightToggleData.Create(PlayerIdManager.LocalSmallId, syncable.Id, __instance.lightOn)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.FlashlightToggle, writer)) {
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
