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
    public sealed class PointShopPlacer : FusionMarrowBehaviour {
#if MELONLOADER
        public PointShopPlacer(IntPtr intPtr) : base(intPtr) { }

        public void Start() {
            PointShopHelper.SetupPointShop(transform.position, transform.rotation, transform.lossyScale);
        }
#else
        public override string Comment => "Allows you to place the BitMart anywhere within your map!\n" +
            "If you have Gizmos enabled, you can see the shape of the BitMart.\n" +
            "The BitMart is affected by scale, position, and rotation.\n" +
            "The BitMart will be created when the scene loads, and you do not have to do anything extra.";

        private void OnDrawGizmos() {
            // Draw a bitmart representation
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawWireCube(new Vector3(0.09725136f, 1.511268f, 0f), new Vector3(1.875499f, 3.000952f, 1f));
            Gizmos.DrawWireCube(new Vector3(-1.115174f, 0.6044478f, 0.001287244f), new Vector3(0.5468338f, 1.187312f, 0.3292807f));
        }
#endif
    }
}
