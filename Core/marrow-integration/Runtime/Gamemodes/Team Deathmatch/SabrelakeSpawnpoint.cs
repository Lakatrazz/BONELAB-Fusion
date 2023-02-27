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
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Sabrelake Spawnpoint")]
    [DisallowMultipleComponent]
#endif
    public sealed class SabrelakeSpawnpoint : MonoBehaviour {
#if MELONLOADER
        public SabrelakeSpawnpoint(IntPtr intPtr) : base(intPtr) { }

        public static readonly FusionComponentCache<GameObject, SabrelakeSpawnpoint> Cache = new FusionComponentCache<GameObject, SabrelakeSpawnpoint>();

        private void Awake() {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy() {
            Cache.Remove(gameObject);
        }
#endif
    }
}
