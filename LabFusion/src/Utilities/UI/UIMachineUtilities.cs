using LabFusion.Extensions;
using LabFusion.UI;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Utilities;

using Il2CppTMPro;
using Il2CppSLZ.Marrow.Zones;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Data;
using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Utilities
{
    public static class UIMachineUtilities
    {
        public static void CreateLaserCursor(Transform canvas, Transform uiPlane, Vector3 bounds)
        {
            LaserCursorUtilities.CreateLaserCursor((cursor) =>
            {
                cursor.transform.parent = canvas.parent;
                cursor.transform.SetLocalPositionAndRotation(Vector3Extensions.zero, QuaternionExtensions.identity);

                LaserCursor.CursorRegion region = new()
                {
                    bounds = new Bounds(Vector3Extensions.zero, Vector3.Scale(bounds, canvas.lossyScale)),
                    center = uiPlane,
                };

                cursor.regions = new LaserCursor.CursorRegion[] { region };

                FusionSceneManager.HookOnLevelLoad(() =>
                {
                    cursor.gameObject.SetActive(true);
                });
            });
        }

        public static void CreateUITrigger(GameObject canvas, GameObject trigger)
        {
            trigger.SetActive(false);
            trigger.layer = (int)MarrowLayers.EntityTrigger;
            var zone = trigger.AddComponent<Zone>();
            var zoneEvent = trigger.AddComponent<ZoneEvents>();

            zoneEvent._zone = zone;

            zoneEvent.onZoneEnter = new ZoneEvents.ZoneEventCallback();
            zoneEvent.onZoneEnterOneShot = new ZoneEvents.ZoneEventCallback();
            zoneEvent.onZoneExit = new ZoneEvents.ZoneEventCallback();

            var tagQuery = new TagQuery
            {
                BoneTag = new BoneTagReference("SLZ.Marrow.BoneTag.Player")
            };

            zoneEvent.activatorTags.Tags.Add(tagQuery);

            zoneEvent.onZoneEnter.add_DynamicCalls((Il2CppSystem.Action<MarrowEntity>)((e) => {
                canvas.SetActive(true);
                FusionAudio.Play3D(canvas.transform.position, FusionContentLoader.UITurnOn.Asset);
            }));

            zoneEvent.onZoneExit.add_DynamicCalls((Il2CppSystem.Action<MarrowEntity>)((e) => {
                canvas.SetActive(false);
                FusionAudio.Play3D(canvas.transform.position, FusionContentLoader.UITurnOff.Asset);
            }));

            trigger.SetActive(true);
        }

        public static void OverrideFonts(Transform root)
        {
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                text.font = PersistentAssetCreator.Font;
            }
        }

        public static void AddButtonTriggers(Transform root)
        {
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                var collider = button.GetComponentInChildren<Collider>(true);
                if (collider != null)
                {
                    var interactor = collider.gameObject.AddComponent<FusionUITrigger>();
                    interactor.button = button;
                }
            }
        }

        public static void AddClickEvent(this Button button, Action action)
        {
            button.onClick.AddListener((UnityAction)action);
        }
    }
}