using System;
using System.Runtime.InteropServices;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.NativeStructs;
using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Senders;
using LabFusion.Representation;

using MelonLoader;

using UnityEngine;

using System.Collections;

using SLZ.AI;
using SLZ.Rig;
using SLZ.Marrow.Data;

namespace LabFusion.Patching
{
    // Since some methods here use structs, we native patch thanks to IL2CPP nonsense
    public static class PlayerDamageReceiverPatches
    {
        public static void Patch() {
            PatchReceiveAttack();
        }

        // ReceiveAttack patching stuff
        private static ReceiveAttackPatchDelegate _original;

        public delegate void ReceiveAttackPatchDelegate(IntPtr instance, IntPtr attack, IntPtr method);

        private unsafe static void PatchReceiveAttack()
        {
            ReceiveAttackPatchDelegate patch = ReceiveAttack;

            // Mouthful
            string nativeInfoName = "NativeMethodInfoPtr_ReceiveAttack_Public_Virtual_Final_New_Void_Attack_0";

            var tgtPtr = *(IntPtr*)(IntPtr)typeof(PlayerDamageReceiver).GetField(nativeInfoName, AccessTools.all).GetValue(null);
            var dstPtr = patch.Method.MethodHandle.GetFunctionPointer();

            MelonUtils.NativeHookAttach((IntPtr)(&tgtPtr), dstPtr);
            _original = Marshal.GetDelegateForFunctionPointer<ReceiveAttackPatchDelegate>(tgtPtr);
        }

        private static void ReceiveAttack(IntPtr instance, IntPtr attack, IntPtr method)
        {
            try
            {
                if (NetworkInfo.HasServer)
                {
                    unsafe
                    {
                        var receiver = new PlayerDamageReceiver(instance);
                        var rm = receiver.health._rigManager;

                        // Get the attack
                        var _attack = *(Attack_*)attack;
                        var triggerRef = new TriggerRefProxy(_attack.proxy);

                        // Make sure we have a rigmanager and a proxy
                        if (rm != null && triggerRef != null && _attack.attackType == AttackType.Piercing) {
                            // Check if this is our rigmanager or another player's
                            if (rm == RigData.RigReferences.RigManager) {
                                // Check if a player rep shot the bullet
                                if (triggerRef.root) {
                                    var otherRig = RigManager.Cache.Get(triggerRef.root);

                                    if (otherRig != null && PlayerRepManager.HasPlayerId(otherRig))
                                        return;
                                }
                            }
                            // If this is a player rep, check if we shot the bullet
                            else if (PlayerRepManager.TryGetPlayerRep(rm, out var rep) && triggerRef == RigData.RigReferences.Proxy) {
                                // Send the damage over the network
                                PlayerSender.SendPlayerDamage(rep.PlayerId, _attack.damage);
                            }
                        }
                    }
                }

                _original(instance, attack, method);
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("executing native patch PlayerDamageReceiver.ReceiveAttack", e);
#endif
            }
        }

        private static IEnumerator CoWaitAndSync(Rigidbody rb)
        {
            for (var i = 0; i < 4; i++)
                yield return null;

            PropSender.SendPropCreation(rb.gameObject);
        }
    }
}
