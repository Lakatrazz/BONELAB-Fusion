using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.SDK.Points;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Spawners/Point Shop Placer")]
    [DisallowMultipleComponent]
#endif
    public sealed class PointShopPlacer : MonoBehaviour {
#if MELONLOADER
        public PointShopPlacer(IntPtr intPtr) : base(intPtr) { }

        public void Start() {
            PointShopHelper.SetupPointShop(transform.position, transform.rotation, transform.lossyScale);
        }
#endif
    }
}
