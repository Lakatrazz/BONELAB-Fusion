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
    [AddComponentMenu("BONELAB Fusion/Invoke Ult Event If Host")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UltEventHolder))]
#endif
    public sealed class InvokeUltEventIfHost : MonoBehaviour {
#if !ENABLE_MONO && !ENABLE_IL2CPP
        public InvokeUltEventIfHost(IntPtr intPtr) : base(intPtr) { }
        
        private void Start() {
            var holder = GetComponent<UltEventHolder>();

            if (NetworkInfo.IsServer && holder != null)
                holder.Invoke();
        }
#endif
    }
}
