using System;

using UnityEngine;

#if !ENABLE_MONO && !ENABLE_IL2CPP
using MelonLoader;

using LabFusion.Network;
using LabFusion.Senders;
#endif

namespace LabFusion.MarrowIntegration {
#if !ENABLE_MONO && !ENABLE_IL2CPP
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Auto Sync On Start")]
    [DisallowMultipleComponent]
#endif
    public sealed class AutoSyncOnStart : MonoBehaviour {
#if !ENABLE_MONO && !ENABLE_IL2CPP
        public AutoSyncOnStart(IntPtr intPtr) : base(intPtr) { }

        public void Start()
        {
            if (NetworkInfo.IsServer)
                PropSender.SendPropCreation(gameObject);
        }
#else
        public void Start() 
        {
        }
#endif
    }
}
