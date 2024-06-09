using LabFusion.Utilities;

using MelonLoader;

using UnityEngine;
using UnityEngine.UI;

using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace LabFusion.UI
{
    [RegisterTypeInIl2Cpp]
    public sealed class FusionUITrigger : MonoBehaviour
    {
        public const float InteractionVolume = 0.4f;

        public FusionUITrigger(IntPtr intPtr) : base(intPtr) { }

        public Button button;

        private void Awake()
        {
            var eventTrigger = gameObject.AddComponent<EventTrigger>();

            // Clicking
            EventTrigger.Entry click = new()
            {
                eventID = EventTriggerType.PointerClick
            };
            click.callback.AddListener((UnityAction<BaseEventData>)((eventData) => { FusionAudio.Play3D(transform.position, FusionContentLoader.UIConfirm.Asset, InteractionVolume); button.onClick?.Invoke(); }));

            // Hovering
            EventTrigger.Entry hover = new()
            {
                eventID = EventTriggerType.PointerEnter
            };
            hover.callback.AddListener((UnityAction<BaseEventData>)((eventData) => { FusionAudio.Play3D(transform.position, FusionContentLoader.UISelect.Asset, InteractionVolume); }));


            eventTrigger.delegates.Add(click);
            eventTrigger.delegates.Add(hover);
        }
    }
}