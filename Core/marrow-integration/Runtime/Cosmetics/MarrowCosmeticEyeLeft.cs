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
    [AddComponentMenu("BONELAB Fusion/Cosmetics/Marrow Cosmetic Eye Left")]
    [DisallowMultipleComponent]
#endif
    public sealed class MarrowCosmeticEyeLeft : MarrowCosmeticPoint {
#if MELONLOADER
        public MarrowCosmeticEyeLeft(IntPtr intPtr) : base(intPtr) { }

        public override AccessoryPoint Point => AccessoryPoint.EYE_LEFT;
#endif
    }
}