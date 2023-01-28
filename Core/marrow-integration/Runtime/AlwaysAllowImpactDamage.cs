using System;

using UnityEngine;

#if !ENABLE_MONO && !ENABLE_IL2CPP
using MelonLoader;

using LabFusion.Utilities;
#endif

namespace LabFusion.MarrowIntegration {
#if !ENABLE_MONO && !ENABLE_IL2CPP
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Always Allow Impact Damage")]
    [DisallowMultipleComponent]
#endif
    public sealed class AlwaysAllowImpactDamage : MonoBehaviour {
#if !ENABLE_MONO && !ENABLE_IL2CPP
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
