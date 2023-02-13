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

                        // Get the attack and its shooter
                        var _attack = *(Attack_*)attack;
                        TriggerRefProxy proxy = null;

                        if (_attack.proxy != IntPtr.Zero)
                            proxy = new TriggerRefProxy(_attack.proxy);

                        RigManager shooter = null;

                        if (proxy != null && proxy.root != null) {
                            shooter = RigManager.Cache.Get(proxy.root);
                        }

                        // Make sure we have the attacker and attacked
                        if (rm != null && shooter != null) {
                            // Is the attacked person us?
                            if (rm.IsSelf()) {
                                // Were we hit by another player?
                                if (PlayerRepManager.TryGetPlayerRep(shooter, out var rep)) {
                                    FusionPlayer.LastAttacker = rep.PlayerId;

                                    // Only allow manual bullet damage
                                    if (_attack.attackType == AttackType.Piercing) {
                                        return;
                                    }
                                }
                                // Were we hit by ourselves?
                                else {
                                    FusionPlayer.LastAttacker = null;
                                }
                            }
                            // Is the attacked person another player? Did we attack them?
                            else if (PlayerRepManager.TryGetPlayerRep(rm, out var rep) && shooter.IsSelf()) {
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
