using System;

using UnityEngine;

using UltEvents;

#if !ENABLE_MONO && !ENABLE_IL2CPP
using MelonLoader;
using LabFusion.Network;
#endif

namespace LabFusion.MarrowIntegration {
#if !ENABLE_MONO && !ENABLE_IL2CPP
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Invoke Ult Event If Client")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UltEventHolder))]
#endif
    public sealed class InvokeUltEventIfClient : MonoBehaviour {
#if !ENABLE_MONO && !ENABLE_IL2CPP
        public InvokeUltEventIfClient(IntPtr intPtr) : base(intPtr) { }
        
        private void Start() {
            var holder = GetComponent<UltEventHolder>();

            if (NetworkInfo.IsClient && holder != null)
                holder.Invoke();
        }
#endif
    }
}
