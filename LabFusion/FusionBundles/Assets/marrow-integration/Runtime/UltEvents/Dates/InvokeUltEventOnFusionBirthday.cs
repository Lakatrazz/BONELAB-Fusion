#if MELONLOADER
using MelonLoader;

using LabFusion.Data;

using Il2CppUltEvents;
#else
using UltEvents;
using UnityEngine;
#endif

namespace LabFusion.MarrowIntegration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/UltEvents/Invoke Ult Event On Fusion Birthday")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UltEventHolder))]
#endif
    public sealed class InvokeUltEventOnFusionBirthday : FusionMarrowBehaviour
    {
#if MELONLOADER
        public InvokeUltEventOnFusionBirthday(IntPtr intPtr) : base(intPtr) { }

        private void Start()
        {
            var holder = GetComponent<UltEventHolder>();

            if (FusionSpecialDates.GetCurrentDate() == FusionSpecialDates.FusionDate.FUSION_BIRTHDAY)
            {
                holder.Invoke();
            }
        }
#else
        public override string Comment => "The UltEventHolder attached to this GameObject will be executed on level load around the time of Fusion's birthday, March 14th.";
#endif
    }
}
