using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.SDK.Points;
using LabFusion.Utilities;
#endif

#if UNITY_EDITOR
using UnityEditor;
using SLZ.Marrow.Utilities;
using SLZ.Marrow;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Spawners/Point Shop Placer")]
    [DisallowMultipleComponent]
#endif
    public sealed class PointShopPlacer : FusionMarrowBehaviour
    {
#if MELONLOADER
        public PointShopPlacer(IntPtr intPtr) : base(intPtr) { }

        public void Start()
        {
            PointShopHelper.SetupPointShop(transform.position, transform.rotation, transform.lossyScale);
        }
#else
        public override string Comment => "Allows you to place the BitMart anywhere within your map!\n" +
            "If you have Gizmos enabled, you can see the shape of the BitMart.\n" +
            "The BitMart is affected by scale, position, and rotation.\n" +
            "The BitMart will be created when the scene loads, and you do not have to do anything extra.";
#endif

#if UNITY_EDITOR
        [DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawPreviewGizmo(PointShopPlacer placer, GizmoType gizmoType)
        {
            if (!Application.isPlaying && placer.gameObject.scene != default)
            {
                var mesh = Resources.Load<Mesh>("Fusion/Mesh/preview_Bitmart");
                if (mesh == null)
                {
                    Debug.LogWarning("Bitmart preview does not exist! Did you install the Fusion SDK properly?");
                    return;
                }

                EditorMeshGizmo.Draw("Bitmart Preview", placer.gameObject, mesh, MarrowSDK.VoidMaterial, mesh.bounds);
            }
        }

        [MenuItem("GameObject/BONELAB Fusion/Spawners/Point Shop Placer", priority = 1)]
        private static void MenuCreatePlacer(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Point Shop Placer", typeof(PointShopPlacer));
            go.transform.localScale = Vector3.one;

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Selection.activeObject = go;
        }
#endif
    }
}
