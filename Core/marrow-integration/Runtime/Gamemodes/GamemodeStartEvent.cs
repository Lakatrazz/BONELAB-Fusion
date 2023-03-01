using System;

using UnityEngine;
using UltEvents;

#if MELONLOADER
using MelonLoader;

using LabFusion.SDK.Gamemodes;
using UnhollowerBaseLib.Attributes;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Gamemode Start Event")]
    [RequireComponent(typeof(UltEventHolder))]
    [DisallowMultipleComponent]
#endif
    public sealed class GamemodeStartEvent : FusionMarrowBehaviour {
#if MELONLOADER
        public GamemodeStartEvent(IntPtr intPtr) : base(intPtr) { }

        private void Awake() {
            GamemodeManager.OnGamemodeChanged += OnGamemodeChanged;
        }

        private void OnDestroy() {
            GamemodeManager.OnGamemodeChanged -= OnGamemodeChanged;
        }

        [HideFromIl2Cpp]
        private void OnGamemodeChanged(Gamemode gamemode) {
            if (gamemode != null) {
                var holder = GetComponent<UltEventHolder>();

                if (holder != null)
                    holder.Invoke();
            }
        }
#else
        public override string Comment => "Executes the UltEventHolder attached to this GameObject when a gamemode starts.";
#endif
    }
}
