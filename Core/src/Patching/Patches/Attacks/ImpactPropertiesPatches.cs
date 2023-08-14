using System;
using System.Runtime.InteropServices;
using System.Collections;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.NativeStructs;
using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Senders;

using MelonLoader;

using SLZ.Combat;
using SLZ.Marrow.Data;
using SLZ.AI;

using UnityEngine;

namespace LabFusion.Patching
{
    // Since some methods here use structs, we native patch thanks to IL2CPP nonsense
    public static class ImpactPropertiesPatches
    {
        public static void Patch() {
            PatchReceiveAttack();
        }

        // ReceiveAttack patching stuff
        private static ReceiveAttackPatchDelegate _original;

        private unsafe static void PatchReceiveAttack()
        {
            var tgtPtr = NativeUtilities.GetNativePtr<ImpactProperties>("NativeMethodInfoPtr_ReceiveAttack_Public_Virtual_Final_New_Void_Attack_0");
            var dstPtr = NativeUtilities.GetDestPtr<ReceiveAttackPatchDelegate>(ReceiveAttack);

            MelonUtils.NativeHookAttach((IntPtr)(&tgtPtr), dstPtr);
            _original = NativeUtilities.GetOriginal<ReceiveAttackPatchDelegate>(tgtPtr);
        }

        private static void ReceiveAttack(IntPtr instance, IntPtr attack, IntPtr method)
        {
            try
            {
                if (NetworkInfo.HasServer)
                {
                    unsafe
                    {
                        var _attack = *(Attack_*)attack;

                        Collider collider = null;
                        TriggerRefProxy proxy = null;

                        if (_attack.collider != IntPtr.Zero)
                            collider = new Collider(_attack.collider);

                        if (_attack.proxy != IntPtr.Zero)
                            proxy = new TriggerRefProxy(_attack.proxy);

                        // Check if this was a bullet attack + it was us who shot the bullet
                        if (proxy == RigData.RigReferences.Proxy && _attack.attackType == AttackType.Piercing) {
                            var rb = collider.attachedRigidbody;
                            if (!rb)
                                return;

                            ImpactUtilities.OnHitRigidbody(rb);
                        }
                    }
                }

                _original(instance, attack, method);
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("executing native patch ImpactProperties.ReceiveAttack", e);
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
