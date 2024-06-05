using LabFusion.Extensions;
using LabFusion.UI;

using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using SLZ.Bonelab;
using SLZ.UI;
using SLZ.Utilities;

using TMPro;

namespace LabFusion.Utilities
{
    public static class UIMachineUtilities
    {
        public static void CreateLaserCursor(Transform canvas, Transform uiPlane, Vector3 bounds)
        {
            LaserCursorUtilities.CreateLaserCursor((cursor) =>
            {
                cursor.transform.parent = canvas.parent;
                cursor.transform.localPosition = Vector3Extensions.zero;
                cursor.transform.localRotation = QuaternionExtensions.identity;

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
            var triggerLasers = trigger.AddComponent<TriggerLasers>();
            triggerLasers.LayerFilter = LayerMask.GetMask(new string[] { "Trigger", });
            triggerLasers.onlyTriggerOnPlayer = true;
            triggerLasers.OnTriggerEnterEvent = new UnityEventTrigger();
            triggerLasers.OnTriggerExitEvent = new UnityEventTrigger();

            triggerLasers.OnTriggerEnterEvent.AddCall(UnityEvent.GetDelegate((UnityAction)(() =>
            {
                canvas.SetActive(true);
                FusionAudio.Play3D(canvas.transform.position, FusionContentLoader.UITurnOn.Asset);
            })));

            triggerLasers.OnTriggerExitEvent.AddCall(UnityEvent.GetDelegate((UnityAction)(() =>
            {
                canvas.SetActive(false);
                FusionAudio.Play3D(canvas.transform.position, FusionContentLoader.UITurnOff.Asset);
            })));

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