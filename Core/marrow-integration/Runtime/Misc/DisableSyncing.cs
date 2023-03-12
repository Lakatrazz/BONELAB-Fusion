using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Disable Syncing")]
    [DisallowMultipleComponent]
#endif
    public sealed class DisableSyncing : FusionMarrowBehaviour {
#if MELONLOADER
        public DisableSyncing(IntPtr intPtr) : base(intPtr) { }
#else
        public override string Comment => "This script prevents a rigidbody object or spawnable object from ever being synced.";
#endif
    }
}
