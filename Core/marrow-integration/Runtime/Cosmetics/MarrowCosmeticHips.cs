﻿using System;
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
    [AddComponentMenu("BONELAB Fusion/Cosmetics/Marrow Cosmetic Hips")]
    [DisallowMultipleComponent]
#endif
    public sealed class MarrowCosmeticHips : MarrowCosmeticPoint {
#if MELONLOADER
        public MarrowCosmeticHips(IntPtr intPtr) : base(intPtr) { }

        public override AccessoryPoint Point => AccessoryPoint.HIPS;
#endif
    }
}