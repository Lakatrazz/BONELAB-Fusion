using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.Extensions;

using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppSLZ.Marrow.Warehouse;
#else
using SLZ.Marrow;
using SLZ.Marrow.Utilities;
using SLZ.Marrow.Warehouse;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [DisallowMultipleComponent]
#endif
    public sealed class GamemodeMarker : MonoBehaviour
    {
#if MELONLOADER
        public GamemodeMarker(IntPtr intPtr) : base(intPtr) { }

        public static readonly HashSet<GamemodeMarker> Markers = new(new UnityComparer());

        public Il2CppReferenceField<TagList> teamTags;

        private TagList _teamTagsCached = null;

        public TagList TeamTags => _teamTagsCached;

        private void Awake()
        {
            _teamTagsCached = teamTags.Get();

            Markers.Add(this);
        }

        private void OnDestroy()
        {
            Markers.Remove(this);
        }

        public static IReadOnlyList<GamemodeMarker> FilterMarkers(BoneTagReference tag = null)
        {
            List<GamemodeMarker> markers = new();

            foreach (var marker in Markers)
            {
                if (tag == null)
                {
                    markers.Add(marker);
                    continue;
                }

                if (marker.TeamTags == null)
                {
                    markers.Add(marker);
                    continue;
                }

                var teamTags = marker.TeamTags.Tags;
                bool valid = false;

                foreach (var otherTag in teamTags)
                {
                    if (otherTag.Barcode == tag.Barcode) 
                    {
                        valid = true;
                        break;
                    }
                }

                if (valid)
                {
                    markers.Add(marker);
                    continue;
                }
            }

            return markers;
        }
#else
        public TagList teamTags;
#endif

#if UNITY_EDITOR
        [DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawPreviewGizmo(GamemodeMarker marker, GizmoType gizmoType)
        {
            if (!Application.isPlaying && marker.gameObject.scene != default)
            {
                EditorMeshGizmo.Draw("Gamemode Marker Preview", marker.gameObject, MarrowSDK.GenericHumanMesh, MarrowSDK.VoidMaterialAlt, MarrowSDK.GenericHumanMesh.bounds);
            }
        }

        [MenuItem("GameObject/Fusion/Gamemodes/Gamemode Marker", priority = 1)]
        private static void MenuCreatePlacer(MenuCommand menuCommand)
        {
            GameObject go = new("Gamemode Marker", typeof(GamemodeMarker));
            go.transform.localScale = Vector3.one;

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Selection.activeObject = go;
        }
#endif
    }
}
