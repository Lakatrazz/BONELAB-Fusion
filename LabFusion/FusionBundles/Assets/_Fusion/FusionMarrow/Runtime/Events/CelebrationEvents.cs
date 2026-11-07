using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.Data;

using Il2CppUltEvents;

using Il2CppInterop.Runtime.InteropTypes.Fields;
#else
using UltEvents;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [DisallowMultipleComponent]
#endif
    public sealed class CelebrationEvents : MonoBehaviour
    {
#if MELONLOADER
        public CelebrationEvents(IntPtr intPtr) : base(intPtr) { }

        public Il2CppReferenceField<UltEventHolder> onModBirthdayHolder;

        private void Start()
        {
            if (FusionSpecialDates.GetCurrentDate() == FusionSpecialDates.FusionDate.FUSION_BIRTHDAY)
            {
                onModBirthdayHolder.Get()?.Invoke();
            }
        }
#else
        public UltEventHolder onModBirthdayHolder;
#endif
    }
}
