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
    [AddComponentMenu("BONELAB Fusion/Cosmetics/Marrow Cosmetic Chest Back")]
    [DisallowMultipleComponent]
#endif
    public sealed class MarrowCosmeticChestBack : MarrowCosmeticPoint
    {
#if MELONLOADER
        public MarrowCosmeticChestBack(IntPtr intPtr) : base(intPtr) { }

        public override AccessoryPoint Point => AccessoryPoint.CHEST_BACK;
#endif
    }
}