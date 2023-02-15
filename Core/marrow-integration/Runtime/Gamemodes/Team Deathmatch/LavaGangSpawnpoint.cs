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
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Lava Gang Spawnpoint")]
    [DisallowMultipleComponent]
#endif
    public sealed class LavaGangSpawnpoint : MonoBehaviour {
#if !ENABLE_MONO && !ENABLE_IL2CPP
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
