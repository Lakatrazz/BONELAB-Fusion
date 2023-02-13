using System;
using System.Runtime.InteropServices;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.NativeStructs;
using LabFusion.Utilities;

using MelonLoader;

using SLZ.Combat;
using SLZ.Marrow.Data;
using SLZ.AI;

using UnityEngine;
using LabFusion.Data;
using LabFusion.Senders;
using System.Collections;
using LabFusion.Syncables;
using static UnityEngine.ParticleSystem.PlaybackState;

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

        public delegate void ReceiveAttackPatchDelegate(IntPtr instance, IntPtr attack, IntPtr method);

        private unsafe static void PatchReceiveAttack()
        {
            ReceiveAttackPatchDelegate patch = ReceiveAttack;

            // Mouthful
            string nativeInfoName = "NativeMethodInfoPtr_ReceiveAttack_Public_Virtual_Final_New_Void_Attack_0";

            var tgtPtr = *(IntPtr*)(IntPtr)typeof(ImpactProperties).GetField(nativeInfoName, AccessTools.all).GetValue(null);
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
