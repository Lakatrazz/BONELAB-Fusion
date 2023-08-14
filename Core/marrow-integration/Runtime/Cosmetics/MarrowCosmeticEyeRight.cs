using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if MELONLOADER
using MelonLoader;
using LabFusion.SDK.Points;
#endif

namespace LabFusion.MarrowIntegration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Cosmetics/Marrow Cosmetic Eye Right")]
    [DisallowMultipleComponent]
#endif
    public sealed class MarrowCosmeticEyeRight : MarrowCosmeticPoint {
#if MELONLOADER
        public MarrowCosmeticEyeRight(IntPtr intPtr) : base(intPtr) { }

        public override AccessoryPoint Point => AccessoryPoint.EYE_RIGHT;
#endif
    }
}