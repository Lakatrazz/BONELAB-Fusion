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
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Lava Gang Spawnpoint")]
    [DisallowMultipleComponent]
#endif
    public sealed class LavaGangSpawnpoint : FusionMarrowBehaviour {
#if MELONLOADER
        public LavaGangSpawnpoint(IntPtr intPtr) : base(intPtr) { }

        public static readonly FusionComponentCache<GameObject, LavaGangSpawnpoint> Cache = new FusionComponentCache<GameObject, LavaGangSpawnpoint>();

        private void Awake() {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy() {
            Cache.Remove(gameObject);
        }
#else
        public override string Comment => "Creates a spawn point for players on the Lava Gang team during Team Deathmatch.\n" +
            "You can have as many of these in your scene as you want, and it will become a random spawn.";
#endif

#if UNITY_EDITOR                       
        [DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawPreviewGizmo(LavaGangSpawnpoint spawnpoint, GizmoType gizmoType)
        {
            if (!Application.isPlaying && spawnpoint.gameObject.scene != default)
            {
                EditorMeshGizmo.Draw("Lava Gang Spawnpoint Preview", spawnpoint.gameObject, MarrowSDK.GenericHumanMesh, MarrowSDK.VoidMaterial, MarrowSDK.GenericHumanMesh.bounds);
            }
        }

        [MenuItem("GameObject/BONELAB Fusion/Gamemodes/Lava Gang Spawnpoint", priority = 1)]
        private static void MenuCreatePlacer(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Lava Gang Spawnpoint", typeof(LavaGangSpawnpoint));
            go.transform.localScale = Vector3.one;

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Selection.activeObject = go;
        }
#endif
    }
}
