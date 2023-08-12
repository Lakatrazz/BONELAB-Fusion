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
    [AddComponentMenu("BONELAB Fusion/Spawners/Cup Board Placer")]
    [DisallowMultipleComponent]
#endif
    public sealed class CupBoardPlacer : FusionMarrowBehaviour
    {
#if MELONLOADER
        public CupBoardPlacer(IntPtr intPtr) : base(intPtr) { }

        public void Start()
        {
            FusionSceneManager.HookOnLevelLoad(() => {
                CupBoardHelper.SetupCupBoard(transform.position, transform.rotation, transform.lossyScale);
            });
        }
#else
        public override string Comment => "Allows you to place the Achievement Machine anywhere within your map!\n" +
            "If you have Gizmos enabled, you can see the shape of the Achievement Machine.\n" +
            "The Achievement Machine is affected by scale, position, and rotation.\n" +
            "The Achievement Machine will be created when the scene loads, and you do not have to do anything extra.";
#endif

#if UNITY_EDITOR
        [DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawPreviewGizmo(CupBoardPlacer placer, GizmoType gizmoType)
        {
            if (!Application.isPlaying && placer.gameObject.scene != default)
            {
                var mesh = Resources.Load<Mesh>("Fusion/Mesh/preview_CupBoard");
                if (mesh == null)
                {
                    Debug.LogWarning("Achievement Machine preview does not exist! Did you install the Fusion SDK properly?");
                    return;
                }

                EditorMeshGizmo.Draw("Achievement Machine Preview", placer.gameObject, mesh, MarrowSDK.VoidMaterial, mesh.bounds);
            }
        }

        [MenuItem("GameObject/BONELAB Fusion/Spawners/Cup Board Placer", priority = 1)]
        private static void MenuCreatePlacer(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Cup Board Placer", typeof(CupBoardPlacer));
            go.transform.localScale = Vector3.one;

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Selection.activeObject = go;
        }
#endif
    }
}
