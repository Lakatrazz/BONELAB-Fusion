using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.Marrow;
#endif

#if UNITY_EDITOR
using UnityEditor;

using SLZ.Marrow.Utilities;
using SLZ.Marrow;
#endif

namespace LabFusion.MarrowIntegration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Team Spawnpoint")]
    [DisallowMultipleComponent]
#endif
    public sealed class TeamSpawnpoint : FusionMarrowBehaviour
    {
#if MELONLOADER
        public TeamSpawnpoint(IntPtr intPtr) : base(intPtr) { }

        public static readonly FusionComponentCache<GameObject, TeamSpawnpoint> Cache = new FusionComponentCache<GameObject, TeamSpawnpoint>();

        public string TeamName { get; private set; }

        private void Awake()
        {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy()
        {
            Cache.Remove(gameObject);
        }

        public void SetTeam(string teamName)
        {
            TeamName = teamName;
        }
#else
        public override string Comment => "Creates a spawn point for players on a team during Team Deathmatch.\n" +
            "You can have as many of these in your scene as you want, and it will become a random spawn.";

        public void SetTeam(string teamName) { }
#endif

#if UNITY_EDITOR        
        [DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawPreviewGizmo(TeamSpawnpoint spawnpoint, GizmoType gizmoType)
        {
            if (!Application.isPlaying && spawnpoint.gameObject.scene != default)
            {
                EditorMeshGizmo.Draw("Team Spawnpoint Preview", spawnpoint.gameObject, MarrowSDK.GenericHumanMesh, MarrowSDK.VoidMaterial, MarrowSDK.GenericHumanMesh.bounds);
            }
        }

        [MenuItem("GameObject/BONELAB Fusion/Gamemodes/Team Spawnpoint", priority = 1)]
        private static void MenuCreatePlacer(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Team Spawnpoint", typeof(TeamSpawnpoint));
            go.transform.localScale = Vector3.one;

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Selection.activeObject = go;
        }
#endif
    }
}
