using System;

using UnityEngine;

#if !ENABLE_MONO && !ENABLE_IL2CPP
using MelonLoader;

using LabFusion.Utilities;
#endif

namespace LabFusion.MarrowIntegration
{
#if !ENABLE_MONO && !ENABLE_IL2CPP
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Only Trigger On Local Player")]
    [DisallowMultipleComponent]
#endif
    public sealed class OnlyTriggerOnLocalPlayer : MonoBehaviour
    {
#if !ENABLE_MONO && !ENABLE_IL2CPP
        public OnlyTriggerOnLocalPlayer(IntPtr intPtr) : base(intPtr) { }

        public static readonly FusionComponentCache<GameObject, OnlyTriggerOnLocalPlayer> Cache = new FusionComponentCache<GameObject, OnlyTriggerOnLocalPlayer>();

        private void Awake()
        {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy()
        {
            Cache.Remove(gameObject);
        }
#endif
    }
}
