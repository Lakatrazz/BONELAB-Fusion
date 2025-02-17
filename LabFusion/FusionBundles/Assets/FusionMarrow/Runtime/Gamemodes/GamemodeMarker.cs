using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.Extensions;

using Il2CppSLZ.Marrow.Warehouse;

using Il2CppInterop.Runtime.Attributes;
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
    [HelpURL("https://github.com/Lakatrazz/BONELAB-Fusion/wiki/Gamemode-Maps#gamemode-markers")]
#endif
    public sealed class GamemodeMarker : MonoBehaviour
    {
#if MELONLOADER
        public GamemodeMarker(IntPtr intPtr) : base(intPtr) { }

        public static readonly HashSet<GamemodeMarker> Markers = new(new UnityComparer());

        private readonly HashSet<string> _teamBarcodes = new();

        [HideFromIl2Cpp]
        public HashSet<string> TeamBarcodes => _teamBarcodes; 

        private void Awake()
        {
            Markers.Add(this);
        }

        private void OnDestroy()
        {
            Markers.Remove(this);
        }

        public void AddTeam(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return;
            }

            _teamBarcodes.Add(barcode);
        }

        public void RemoveTeam(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return;
            }

            _teamBarcodes.Remove(barcode);
        }

        public void ClearTeams()
        {
            _teamBarcodes.Clear();
        }

        public static IReadOnlyList<GamemodeMarker> FilterMarkers(BoneTagReference tag = null)
        {
            List<GamemodeMarker> markers = new();

            bool tagIsValid = tag != null;
            string tagBarcode = tagIsValid ? tag.Barcode.ID : Barcode.EMPTY;

            foreach (var marker in Markers)
            {
                bool markerHasTeam = marker.TeamBarcodes != null && marker.TeamBarcodes.Count > 0;

                // If filtering for a null tag and the marker has no teams, the marker should be valid
                if (!tagIsValid && !markerHasTeam) 
                {
                    markers.Add(marker);
                    continue;
                }

                var teamBarcodes = marker.TeamBarcodes;
                bool valid = false;

                foreach (var otherBarcode in teamBarcodes)
                {
                    if (otherBarcode == tagBarcode)
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

        public void AddTeam(string barcode)
        {
        }

        public void RemoveTeam(string barcode) 
        { 
        }

        public void ClearTeams()
        {
        }
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
