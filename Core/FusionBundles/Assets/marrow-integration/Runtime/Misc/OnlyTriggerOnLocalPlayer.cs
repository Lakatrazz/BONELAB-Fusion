using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.Utilities;
#endif

namespace LabFusion.MarrowIntegration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Only Trigger On Local Player")]
    [DisallowMultipleComponent]
#endif
    public sealed class OnlyTriggerOnLocalPlayer : FusionMarrowBehaviour
    {
#if MELONLOADER
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
#else
        public override string Comment => "This script will prevent a TriggerLasers component from triggering from other players.\n" +
            "Useful when you need to detect when the local player specifically enters an area.";
#endif
    }
}
