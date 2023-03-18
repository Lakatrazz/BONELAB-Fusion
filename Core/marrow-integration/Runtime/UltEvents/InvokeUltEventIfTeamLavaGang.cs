using System;

using UnityEngine;

using UltEvents;

#if MELONLOADER
using MelonLoader;
using LabFusion.Utilities;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/UltEvents/Invoke Ult Event If Team Lava Gang")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UltEventHolder))]
#endif
    public sealed class InvokeUltEventIfTeamLavaGang : FusionMarrowBehaviour {
#if MELONLOADER
        public InvokeUltEventIfTeamLavaGang(IntPtr intPtr) : base(intPtr) { }

        public static readonly FusionComponentCache<GameObject, InvokeUltEventIfTeamLavaGang> Cache = new FusionComponentCache<GameObject, InvokeUltEventIfTeamLavaGang>();

        private void Awake() {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy() {
            Cache.Remove(gameObject);
        }

        public void Invoke() {
            var holder = GetComponent<UltEventHolder>();

            if (holder != null)
                holder.Invoke();
        }
#else
        public override string Comment => "The UltEventHolder attached to this GameObject will be executed when the local player becomes part of Lava Gang in Team Deathmatch.";
#endif
    }
}
