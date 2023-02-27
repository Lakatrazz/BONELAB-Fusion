using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.Utilities;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Always Allow Impact Damage")]
    [DisallowMultipleComponent]
#endif
    public sealed class AlwaysAllowImpactDamage : MonoBehaviour {
#if MELONLOADER
        public AlwaysAllowImpactDamage(IntPtr intPtr) : base(intPtr) { }

        public static readonly FusionComponentCache<GameObject, AlwaysAllowImpactDamage> Cache = new FusionComponentCache<GameObject, AlwaysAllowImpactDamage>();

        private void Awake() {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy() {
            Cache.Remove(gameObject);
        }
#endif
    }
}
