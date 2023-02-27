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
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Lava Gang Spawnpoint")]
    [DisallowMultipleComponent]
#endif
    public sealed class LavaGangSpawnpoint : MonoBehaviour {
#if MELONLOADER
        public LavaGangSpawnpoint(IntPtr intPtr) : base(intPtr) { }

        public static readonly FusionComponentCache<GameObject, LavaGangSpawnpoint> Cache = new FusionComponentCache<GameObject, LavaGangSpawnpoint>();

        private void Awake() {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy() {
            Cache.Remove(gameObject);
        }
#endif
    }
}
