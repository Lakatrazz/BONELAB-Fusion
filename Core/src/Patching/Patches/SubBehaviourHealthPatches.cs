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

        // TakeDamage patching stuff
        private static TakeDamagePatchDelegate _original;

        public delegate float TakeDamagePatchDelegate(IntPtr instance, int m, IntPtr attack, IntPtr method);

        private unsafe static void PatchTakeDamage()
        {
            var tgtPtr = NativeUtilities.GetNativePtr<SubBehaviourHealth>("NativeMethodInfoPtr_TakeDamage_Public_Single_Int32_Attack_0");
            var dstPtr = NativeUtilities.GetDestPtr<TakeDamagePatchDelegate>(TakeDamage);

            MelonUtils.NativeHookAttach((IntPtr)(&tgtPtr), dstPtr);
            _original = NativeUtilities.GetOriginal<TakeDamagePatchDelegate>(tgtPtr);
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
