using System;

using UnityEngine;

using UltEvents;

#if MELONLOADER
using MelonLoader;
using LabFusion.Network;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Invoke Ult Event If Host")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UltEventHolder))]
#endif
    public sealed class InvokeUltEventIfHost : MonoBehaviour {
#if MELONLOADER
        public InvokeUltEventIfHost(IntPtr intPtr) : base(intPtr) { }
        
        private void Start() {
            var holder = GetComponent<UltEventHolder>();

            if (NetworkInfo.IsServer && holder != null)
                holder.Invoke();
        }
#endif
    }
}
