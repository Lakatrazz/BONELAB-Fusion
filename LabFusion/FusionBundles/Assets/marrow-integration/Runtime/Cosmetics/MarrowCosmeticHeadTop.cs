#if MELONLOADER
using MelonLoader;
using LabFusion.SDK.Points;
#else
using UnityEngine;
#endif

namespace LabFusion.MarrowIntegration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Cosmetics/Marrow Cosmetic Head Top")]
    [DisallowMultipleComponent]
#endif
    public sealed class MarrowCosmeticHeadTop : MarrowCosmeticPoint
    {
#if MELONLOADER
        public MarrowCosmeticHeadTop(IntPtr intPtr) : base(intPtr) { }

        public override AccessoryPoint Point => AccessoryPoint.HEAD_TOP;
#endif
    }
}