using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;

using SLZ;
using SLZ.Interaction;
using SLZ.Props.Weapons;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(AlignPlug))]
    public static class AlignPlugPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(AlignPlug.OnHandAttached))]
        public static bool OnHandAttached(InteractableHost host, Hand hand) {
            if (NetworkInfo.HasServer && PlayerRepManager.HasPlayerId(hand.manager))
                return false;

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(AlignPlug.OnHandDetached))]
        public static bool OnHandDetached(InteractableHost host, Hand hand) {
            if (NetworkInfo.HasServer && PlayerRepManager.HasPlayerId(hand.manager))
                return false;

            return true;
        }

        [HarmonyPatch(nameof(AlignPlug.OnProxyGrab))]
        [HarmonyPrefix]
        public static bool OnProxyGrab(AlignPlug __instance, Hand hand) {
            if (NetworkInfo.HasServer && __instance.TryCast<AmmoPlug>() && PlayerRepManager.HasPlayerId(hand.manager))
                return false;

            return true;
        }

        [HarmonyPatch(nameof(AlignPlug.OnProxyRelease))]
        [HarmonyPrefix]
        public static bool OnProxyRelease(AlignPlug __instance, Hand hand) {
            if (NetworkInfo.HasServer && __instance.TryCast<AmmoPlug>() && PlayerRepManager.HasPlayerId(hand.manager))
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(AmmoPlug))]
    public static class AmmoPlugPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(AmmoPlug.OnPlugInsertComplete))]
        public static void OnPlugInsertCompletePrefix() {
            AssetPooleePatches.IgnorePatches = true;
            AmmoSocketPatches.IgnorePatch = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(AmmoPlug.OnPlugInsertComplete))]
        public static void OnPlugInsertCompletePostfix() {
            AssetPooleePatches.IgnorePatches = false;
            AmmoSocketPatches.IgnorePatch = false;
        }
    }

    [HarmonyPatch(typeof(AmmoSocket))]
    public static class AmmoSocketPatches {
        public static bool IgnorePatch = false;

        [HarmonyPatch(nameof(AmmoSocket.OnPlugLocked))]
        [HarmonyPrefix]
        public static void OnPlugLocked(AmmoSocket __instance, Plug plug) {
            if (IgnorePatch)
                return;

            try {
                if (NetworkInfo.HasServer && __instance.gun && GunExtender.Cache.TryGet(__instance.gun, out var gunSyncable)) {
                    var ammoPlug = plug.TryCast<AmmoPlug>();

                    if (ammoPlug != null && ammoPlug.magazine && MagazineExtender.Cache.TryGet(ammoPlug.magazine, out var magSyncable)) {
                        
                        using (var writer = FusionWriter.Create(MagazineInsertData.Size)) {
                            using (var data = MagazineInsertData.Create(PlayerIdManager.LocalSmallId, magSyncable.Id, gunSyncable.Id)) {
                                writer.Write(data);

                                using (var message = FusionMessage.Create(NativeMessageTag.MagazineInsert, writer)) {
                                    MessageSender.SendToServer(NetworkChannel.Reliable, message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                FusionLogger.LogException("patching AmmoSocket.OnPlugLocked", e);
            }
        }

        [HarmonyPatch(nameof(AmmoSocket.OnPlugUnlocked))]
        [HarmonyPrefix]
        public static void OnPlugUnlocked(AmmoSocket __instance) {
            if (IgnorePatch)
                return;

            var ammoPlug = __instance._magazinePlug;

            try
            {
                if (NetworkInfo.HasServer && __instance.gun && GunExtender.Cache.TryGet(__instance.gun, out var gunSyncable)) {

                    // Proceed with the ejection
                    if (ammoPlug && ammoPlug.magazine && MagazineExtender.Cache.TryGet(ammoPlug.magazine, out var magSyncable)) {
                        magSyncable.SetRigidbodiesDirty();

                        using (var writer = FusionWriter.Create(MagazineEjectData.Size))
                        {
                            Hand hand = ammoPlug.host.GetHand(0);
                            Handedness handedness = hand != null ? hand.handedness : Handedness.UNDEFINED;

                            using (var data = MagazineEjectData.Create(PlayerIdManager.LocalSmallId, magSyncable.Id, gunSyncable.Id, handedness))
                            {
                                writer.Write(data);

                                using (var message = FusionMessage.Create(NativeMessageTag.MagazineEject, writer))
                                {
                                    MessageSender.SendToServer(NetworkChannel.Reliable, message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException("patching AmmoSocket.OnPlugUnlocked", e);
            }
        }
    }
}
