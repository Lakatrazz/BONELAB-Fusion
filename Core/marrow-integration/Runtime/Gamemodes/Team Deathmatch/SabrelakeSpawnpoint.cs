using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

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
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Sabrelake Spawnpoint")]
    [DisallowMultipleComponent]
#endif
    public sealed class SabrelakeSpawnpoint : FusionMarrowBehaviour {
#if MELONLOADER
        public SabrelakeSpawnpoint(IntPtr intPtr) : base(intPtr) { }

        public static readonly FusionComponentCache<GameObject, SabrelakeSpawnpoint> Cache = new FusionComponentCache<GameObject, SabrelakeSpawnpoint>();

        private void Awake() {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy() {
            Cache.Remove(gameObject);
        }
#else
        public override string Comment => "Creates a spawn point for players on the Sabrelake team during Team Deathmatch.\n" +
            "You can have as many of these in your scene as you want, and it will become a random spawn.";
#endif

#if UNITY_EDITOR                       
        [DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawPreviewGizmo(SabrelakeSpawnpoint spawnpoint, GizmoType gizmoType)
        {
            if (!Application.isPlaying && spawnpoint.gameObject.scene != default)
            {
                EditorMeshGizmo.Draw("Sabrelake Spawnpoint Preview", spawnpoint.gameObject, MarrowSDK.GenericHumanMesh, MarrowSDK.VoidMaterial, MarrowSDK.GenericHumanMesh.bounds);
            }
        }

        [MenuItem("GameObject/BONELAB Fusion/Gamemodes/Sabrelake Spawnpoint", priority = 1)]
        private static void MenuCreatePlacer(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Sabrelake Spawnpoint", typeof(SabrelakeSpawnpoint));
            go.transform.localScale = Vector3.one;

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Selection.activeObject = go;
        }
#endif
    }
}
