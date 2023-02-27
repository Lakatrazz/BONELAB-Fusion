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
    [AddComponentMenu("BONELAB Fusion/Misc/Invoke Ult Event In Server")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UltEventHolder))]
#endif
    public sealed class InvokeUltEventInServer : MonoBehaviour {
#if MELONLOADER
        public InvokeUltEventInServer(IntPtr intPtr) : base(intPtr) { }
        
        private void Start() {
            var holder = GetComponent<UltEventHolder>();

            if (NetworkInfo.HasServer && holder != null)
                holder.Invoke();
        }
#endif
    }
}
