using LabFusion.Utilities;
using LabFusion.Marrow;

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
            click.callback.AddListener((UnityAction<BaseEventData>)((eventData) =>
            { 
                LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.UIConfirmReference), transform.position, LocalAudioPlayer.SFXSettings);

                button.onClick?.Invoke(); 
            }));

            // Hovering
            EventTrigger.Entry hover = new()
            {
                eventID = EventTriggerType.PointerEnter
            };

            hover.callback.AddListener((UnityAction<BaseEventData>)((eventData) => 
            { 
                LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.UISelectReference), transform.position, LocalAudioPlayer.SFXSettings);
            }));


            eventTrigger.delegates.Add(click);
            eventTrigger.delegates.Add(hover);
        }
    }
}