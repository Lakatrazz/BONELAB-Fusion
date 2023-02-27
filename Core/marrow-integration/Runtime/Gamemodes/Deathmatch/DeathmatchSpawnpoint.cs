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
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Deathmatch Spawnpoint")]
    [DisallowMultipleComponent]
#endif
    public sealed class DeathmatchSpawnpoint : MonoBehaviour {
#if MELONLOADER
        public DeathmatchSpawnpoint(IntPtr intPtr) : base(intPtr) { }

        public static readonly FusionComponentCache<GameObject, DeathmatchSpawnpoint> Cache = new FusionComponentCache<GameObject, DeathmatchSpawnpoint>();

        private void Awake() {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy() {
            Cache.Remove(gameObject);
        }
#endif
    }
}
