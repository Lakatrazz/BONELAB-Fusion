using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.Network;
using LabFusion.Senders;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Auto Sync On Start")]
    [DisallowMultipleComponent]
#endif
    public sealed class AutoSyncOnStart : MonoBehaviour {
#if MELONLOADER
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
