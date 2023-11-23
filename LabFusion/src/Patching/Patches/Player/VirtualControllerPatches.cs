﻿using System;
using LabFusion.NativeStructs;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Interaction;

namespace LabFusion.Patching
{
    // Since some methods here use structs, we native patch thanks to IL2CPP nonsense
    public static class VirtualControllerPatches
    {
        public static void Patch()
        {
            PatchCheckHandDesync();
        }

        // CheckHandDesync patching stuff
        private static CheckHandDesyncPatchDelegate _original;

        public delegate bool CheckHandDesyncPatchDelegate(IntPtr instance, IntPtr pair, IntPtr contHandle, IntPtr rigHandle, IntPtr method);

        private unsafe static void PatchCheckHandDesync()
        {
            var tgtPtr = NativeUtilities.GetNativePtr<VirtualController>("NativeMethodInfoPtr_CheckHandDesync_Private_Boolean_HandGripPair_SimpleTransform_SimpleTransform_0");
            var dstPtr = NativeUtilities.GetDestPtr<CheckHandDesyncPatchDelegate>(CheckHandDesync);

            MelonUtils.NativeHookAttach((IntPtr)(&tgtPtr), dstPtr);
            _original = NativeUtilities.GetOriginal<CheckHandDesyncPatchDelegate>(tgtPtr);
        }

        private static bool CheckHandDesync(IntPtr instance, IntPtr pair, IntPtr contHandle, IntPtr rigHandle, IntPtr method)
        {
            try
            {
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
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("executing native patch VirtualController.CheckHandDesync", e);
#endif
                return false;
            }
        }
    }
}
