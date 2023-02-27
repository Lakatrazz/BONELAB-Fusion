using System;

using UnityEngine;

using UltEvents;

#if MELONLOADER
using MelonLoader;

using LabFusion.SDK.Gamemodes;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Gamemode End Event")]
    [RequireComponent(typeof(UltEventHolder))]
    [DisallowMultipleComponent]
#endif
    public sealed class GamemodeEndEvent : MonoBehaviour {
#if MELONLOADER
        public GamemodeEndEvent(IntPtr intPtr) : base(intPtr) { }

        private void Awake() {
            GamemodeManager.OnGamemodeChanged += OnGamemodeChanged;
        }

        private void OnDestroy() {
            GamemodeManager.OnGamemodeChanged -= OnGamemodeChanged;
        }

        private void OnGamemodeChanged(Gamemode gamemode) {
            if (gamemode == null) {
                var holder = GetComponent<UltEventHolder>();

                if (holder != null)
                    holder.Invoke();
            }
        }
#endif
    }
}
