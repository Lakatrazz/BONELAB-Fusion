using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.NativeStructs;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;
using MelonLoader;

using PuppetMasta;

using SLZ;

namespace LabFusion.Patching {
    // Since some methods here use structs, we native patch thanks to IL2CPP nonsense
    public static class SubBehaviourHealthPatches {
        public static void Patch()
        {
            PatchTakeDamage();
        }

        // Stun patching
        [HarmonyPatch(typeof(SubBehaviourHealth), nameof(SubBehaviourHealth.Stun))]
        [HarmonyPrefix]
        public static bool Stun(int m, float stun) {
            return false;
        }

        // TakeDamage patching stuff
        private static TakeDamagePatchDelegate _original;

        public delegate float TakeDamagePatchDelegate(IntPtr instance, int m, IntPtr attack, IntPtr method);

        private unsafe static void PatchTakeDamage()
        {
            TakeDamagePatchDelegate patch = TakeDamage;

            // Mouthful
            string nativeInfoName = "NativeMethodInfoPtr_TakeDamage_Public_Single_Int32_Attack_0";

            var tgtPtr = *(IntPtr*)(IntPtr)typeof(SubBehaviourHealth).GetField(nativeInfoName, AccessTools.all).GetValue(null);
            var dstPtr = patch.Method.MethodHandle.GetFunctionPointer();

            MelonUtils.NativeHookAttach((IntPtr)(&tgtPtr), dstPtr);
            _original = Marshal.GetDelegateForFunctionPointer<TakeDamagePatchDelegate>(tgtPtr);
        }

        private static float TakeDamage(IntPtr instance, int m, IntPtr attack, IntPtr method)
        {
            try {
                if (NetworkInfo.HasServer)
                {
                    SubBehaviourHealth subBehaviourHealth = null;

                    if (instance != IntPtr.Zero)
                        subBehaviourHealth = new SubBehaviourHealth(instance);

                    if (subBehaviourHealth != null && PuppetMasterExtender.Cache.TryGet(subBehaviourHealth.behaviour.puppetMaster, out var syncable) && !syncable.IsOwner()) {
                        return 0f;
                    }
                }

                return _original(instance, m, attack, method);
            }
            catch (Exception e) {
#if DEBUG
                FusionLogger.LogException("executing native patch SubBehaviourHealth.TakeDamage", e);
#endif

                return 0f;
            }
        }
    }
}
