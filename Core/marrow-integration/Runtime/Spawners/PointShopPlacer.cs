using System;

using UnityEngine;

#if !ENABLE_MONO && !ENABLE_IL2CPP
using MelonLoader;

using LabFusion.Points;
#endif

namespace LabFusion.MarrowIntegration {
#if !ENABLE_MONO && !ENABLE_IL2CPP
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Spawners/Point Shop Placer")]
    [DisallowMultipleComponent]
#endif
    public sealed class PointShopPlacer : MonoBehaviour {
#if !ENABLE_MONO && !ENABLE_IL2CPP
        public PointShopPlacer(IntPtr intPtr) : base(intPtr) { }

        public void Start() {
            PointShopHelper.SetupPointShop(transform.position, transform.rotation, transform.lossyScale);
        }
#endif
    }
}
