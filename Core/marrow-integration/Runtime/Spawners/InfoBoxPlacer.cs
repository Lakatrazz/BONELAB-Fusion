using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.UI;
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
    [AddComponentMenu("BONELAB Fusion/Spawners/Info Box Placer")]
    [DisallowMultipleComponent]
#endif
    public sealed class InfoBoxPlacer : FusionMarrowBehaviour
    {
#if MELONLOADER
        public InfoBoxPlacer(IntPtr intPtr) : base(intPtr) { }

        public void Start()
        {
            InfoBoxHelper.SetupInfoBox(transform.position, transform.rotation, transform.lossyScale);
        }
#else
        public override string Comment => "Allows you to place the Info Box anywhere within your map!\n" +
            "If you have Gizmos enabled, you can see the shape of the Info Box.\n" +
            "The Info Box is affected by scale, position, and rotation.\n" +
            "The Info Box will be created when the scene loads, and you do not have to do anything extra.";
#endif

#if UNITY_EDITOR
        [DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawPreviewGizmo(InfoBoxPlacer placer, GizmoType gizmoType)
        {
            if (!Application.isPlaying && placer.gameObject.scene != default)
            {
                var mesh = Resources.Load<Mesh>("Fusion/Mesh/preview_InfoBox");
                if (mesh == null)
                {
                    Debug.LogWarning("Info Box preview does not exist! Did you install the Fusion SDK properly?");
                    return;
                }

                EditorMeshGizmo.Draw("Info Box Preview", placer.gameObject, mesh, MarrowSDK.VoidMaterial, mesh.bounds);
            }
        }

        [MenuItem("GameObject/BONELAB Fusion/Spawners/Info Box Placer", priority = 1)]
        private static void MenuCreatePlacer(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Info Box Placer", typeof(InfoBoxPlacer));
            go.transform.localScale = Vector3.one;

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Selection.activeObject = go;
        }
#endif
    }
}
