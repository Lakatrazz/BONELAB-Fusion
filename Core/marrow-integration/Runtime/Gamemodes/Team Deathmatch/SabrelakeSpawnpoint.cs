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
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Sabrelake Spawnpoint")]
    [DisallowMultipleComponent]
#endif
    public sealed class SabrelakeSpawnpoint : MonoBehaviour {
#if !ENABLE_MONO && !ENABLE_IL2CPP
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
