using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BoneLib;
using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.NativeStructs;

using MelonLoader;

using SLZ;
using SLZ.Combat;
using SLZ.Interaction;
using SLZ.Marrow.Data;
using SLZ.Marrow.Utilities;

using UnityEngine;
using LabFusion.Utilities;
using LabFusion.Data;

namespace LabFusion.Patching
{
    // Since some methods here use structs, we native patch thanks to IL2CPP nonsense
    public static class VirtualControllerPatches {
        public static void Patch() {
            PatchCheckHandDesync();
        }

        // CheckHandDesync patching stuff
        private static CheckHandDesyncPatchDelegate _original;

        public delegate bool CheckHandDesyncPatchDelegate(IntPtr instance, IntPtr pair, IntPtr contHandle, IntPtr rigHandle, IntPtr method);

        private unsafe static void PatchCheckHandDesync() {
            CheckHandDesyncPatchDelegate patch = CheckHandDesync;

            // Mouthful
            string nativeInfoName = "NativeMethodInfoPtr_CheckHandDesync_Private_Boolean_HandGripPair_SimpleTransform_SimpleTransform_0";

            var tgtPtr = *(IntPtr*)(IntPtr)typeof(VirtualController).GetField(nativeInfoName, BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var dstPtr = patch.Method.MethodHandle.GetFunctionPointer();

            MelonUtils.NativeHookAttach((IntPtr)(&tgtPtr), dstPtr);
            _original = Marshal.GetDelegateForFunctionPointer<CheckHandDesyncPatchDelegate>(tgtPtr);
        }

        private static bool CheckHandDesync(IntPtr instance, IntPtr pair, IntPtr contHandle, IntPtr rigHandle, IntPtr method) {
            try {
                if (NetworkInfo.HasServer)
                {
                    unsafe
                    {
                        var _pair = *(HandGripPair_*)pair;
                        Hand hand = null;

                        if (_pair.hand != IntPtr.Zero)
                            hand = new Hand(_pair.hand);

                        if (hand != null && PlayerRepManager.HasPlayerId(hand.manager))
                            return false;
                    }
                }

                return _original(instance, pair, contHandle, rigHandle, method);
            }
            catch (Exception e) {
#if DEBUG
                FusionLogger.LogException("executing native patch VirtualController.CheckHandDesync", e);
#endif
                return false;
            }
        }
    }
}
