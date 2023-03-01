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
    public sealed class AlwaysAllowImpactDamage : FusionMarrowBehaviour {
#if MELONLOADER
        public AlwaysAllowImpactDamage(IntPtr intPtr) : base(intPtr) { }

        public static readonly FusionComponentCache<GameObject, AlwaysAllowImpactDamage> Cache = new FusionComponentCache<GameObject, AlwaysAllowImpactDamage>();

        private void Awake() {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy() {
            Cache.Remove(gameObject);
        }
#else
        public override string Comment => "Placing this onto a GameObject with a script that causes blunt or stab damage will allow it to always damage players without needing to be held.";
#endif
    }
}
