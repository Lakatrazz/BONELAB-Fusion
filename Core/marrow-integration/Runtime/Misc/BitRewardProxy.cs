using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.SDK.Points;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Bit Reward Proxy")]
    [DisallowMultipleComponent]
#endif
    public sealed class BitRewardProxy : FusionMarrowBehaviour {
#if MELONLOADER
        public BitRewardProxy(IntPtr intPtr) : base(intPtr) { }

        public void RewardBits(int bits) {
            PointItemManager.RewardBits(bits);
        }

#else
        public override string Comment => "This proxy lets you reward bits to the user through a UnityEvent or UltEvent.";

        public void RewardBits(int bits) { }
#endif
    }
}
