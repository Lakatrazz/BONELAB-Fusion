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
    [AddComponentMenu("BONELAB Fusion/Invoke Ult Event In Server")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UltEventHolder))]
#endif
    public sealed class InvokeUltEventInServer : MonoBehaviour {
#if !ENABLE_MONO && !ENABLE_IL2CPP
        public InvokeUltEventInServer(IntPtr intPtr) : base(intPtr) { }
        
        private void Start() {
            var holder = GetComponent<UltEventHolder>();

            if (NetworkInfo.HasServer && holder != null)
                holder.Invoke();
        }
#endif
    }
}
