#if MELONLOADER
using MelonLoader;

using LabFusion.Utilities;
#else
using UnityEngine;
#endif

namespace LabFusion.MarrowIntegration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Notification Proxy")]
    [DisallowMultipleComponent]
#endif
    public sealed class NotificationProxy : FusionMarrowBehaviour
    {
#if MELONLOADER
        public NotificationProxy(IntPtr intPtr) : base(intPtr) { }

        public void Send(string title, string message, float length)
        {
            FusionNotifier.Send(new FusionNotification()
            {
                title = title,
                showTitleOnPopup = true,
                isPopup = true,
                isMenuItem = false,
                message = message,
                popupLength = length,
            });
        }

#else
        public override string Comment => "This proxy lets you play pop-up notifications to the user.";

        public void Send(string title, string message, float length) { }
#endif
    }
}
