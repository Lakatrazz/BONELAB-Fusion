using System;

using UnityEngine;

using UltEvents;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/UltEvents/Invoke Ult Event On Other Ownership")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UltEventHolder))]
#endif
    public sealed class InvokeUltEventOnOtherOwnership : FusionMarrowBehaviour {
#if MELONLOADER
        public InvokeUltEventOnOtherOwnership(IntPtr intPtr) : base(intPtr) { }

        private UltEventHolder holder;

        private void Awake() {
            holder = GetComponent<UltEventHolder>();
        }

        public void Invoke() {
            holder.Invoke();
        }
#else
        public override string Comment => "The UltEventHolder attached to this GameObject will be executed when the prop this is attached to becomes owned by another player.";
#endif
    }
}
