using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Data;

using LabFusion.RPC;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.SDK.Messages;
#else
using SLZ.Marrow.Utilities;

using SLZ.Marrow;
using SLZ.Marrow.Warehouse;

using System.Collections.Generic;
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [DisallowMultipleComponent]
    [ExecuteAlways]
#endif
    public class GamemodeDropper : MonoBehaviour
    {
#if MELONLOADER
        public GamemodeDropper(IntPtr intPtr) : base(intPtr) { }

        public static List<Poolee> DroppedItems { get; private set; } = new();

        public static int DroppedItemCount => DroppedItems.Count;

        public static List<GamemodeDropper> Droppers { get; private set; } = new();

        public static bool HasDroppers => Droppers.Count > 0;

        private void Awake()
        {
            Droppers.Add(this);
        }

        private void OnDestroy()
        {
            Droppers.Remove(this);
        }

        public static bool DropItem()
        {
            if (!NetworkInfo.IsHost)
            {
                return false;
            }

            if (!HasDroppers)
            {
                return false;
            }

            var drop = GamemodeDropperSettings.GetItemDrop();

            var randomDropper = Droppers.GetRandom();

            NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
            {
                Position = randomDropper.transform.position,
                Rotation = randomDropper.transform.rotation,
                Spawnable = new Spawnable() { crateRef = drop.ItemCrateReference, policyData = null },
                SpawnEffect = true,
                SpawnCallback = OnItemSpawned,
            });

            return true;
        }

        private static void OnItemSpawned(NetworkAssetSpawner.SpawnCallbackInfo info)
        {
            MessageRelay.RelayModule<GamemodeDropperMessage, GamemodeDropperData>(
                new GamemodeDropperData() { Entity = new(info.Entity.ID) }, CommonMessageRoutes.ReliableToClients);
        }

        public static bool DespawnItems()
        {
            if (!NetworkInfo.IsHost)
            {
                return false;
            }

            foreach (var item in DroppedItems.ToArray()) 
            {
                if (item == null)
                {
                    continue;
                }

                item.Despawn();
            }

            DroppedItems.Clear();

            return true;
        }
#endif

#if UNITY_EDITOR
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                return;
            }

            Handles.color = Color.red;
            Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(Vector3.down), 0.5f, EventType.Repaint);
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawPreviewGizmo(GamemodeDropper dropper, GizmoType gizmoType)
        {
            if (!Application.isPlaying && dropper.gameObject.scene != default)
            {
                var itemDrops = GamemodeDropperSettings.DefaultDrops;

                var dropperSettings = FindObjectOfType<GamemodeDropperSettings>();

                if (dropperSettings != null && dropperSettings.ItemDrops.Count > 0)
                {
                    itemDrops = dropperSettings.ItemDrops;
                }

                var crateReference = GetPreviewCrateReference(itemDrops);

                EditorPreviewMeshGizmo.Draw("Gamemode Dropper Preview", dropper.gameObject, crateReference, MarrowSDK.VoidMaterialAlt);
            }
        }

        private static SpawnableCrateReference GetPreviewCrateReference(List<GamemodeDropperSettings.ItemDrop> itemDrops)
        {
            if (itemDrops.Count <= 0)
            {
                return null;
            }

            var editorSeconds = Mathf.RoundToInt((float)EditorApplication.timeSinceStartup);

            var drop = itemDrops[editorSeconds % itemDrops.Count];

            return drop.ItemCrateReference;
        }
#endif
    }
}