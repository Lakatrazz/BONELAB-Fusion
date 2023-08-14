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
    [AddComponentMenu("BONELAB Fusion/Cosmetics/Marrow Cosmetic Head")]
    [DisallowMultipleComponent]
#endif
    public sealed class MarrowCosmeticHead : MarrowCosmeticPoint {
#if MELONLOADER
        public MarrowCosmeticHead(IntPtr intPtr) : base(intPtr) { }

        public override AccessoryPoint Point => AccessoryPoint.HEAD;
#endif
    }
}