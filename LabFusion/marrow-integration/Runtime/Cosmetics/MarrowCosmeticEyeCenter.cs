﻿using System;
#if MELONLOADER
using MelonLoader;
using LabFusion.SDK.Points;
#endif

namespace LabFusion.MarrowIntegration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Cosmetics/Marrow Cosmetic Eye Center")]
    [DisallowMultipleComponent]
#endif
    public sealed class MarrowCosmeticEyeCenter : MarrowCosmeticPoint
    {
#if MELONLOADER
        public MarrowCosmeticEyeCenter(IntPtr intPtr) : base(intPtr) { }

        public override AccessoryPoint Point => AccessoryPoint.EYE_CENTER;
#endif
    }
}