using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.SDK.Gamemodes;
using UnhollowerBaseLib.Attributes;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Deathmatch Proxy")]
    [DisallowMultipleComponent]
#endif
    public sealed class DeathmatchProxy : MonoBehaviour {
#if MELONLOADER
        public DeathmatchProxy(IntPtr intPtr) : base(intPtr) { }

        public void StartGamemode() {
            if (Deathmatch.Instance != null) {
                Deathmatch.Instance.StartGamemode(true);
            }
        }

        public void StopGamemode() {
            if (Deathmatch.Instance != null) {
                Deathmatch.Instance.StopGamemode();
            }
        }

        public void SetRoundLength(int minutes) {
            if (Deathmatch.Instance != null) {
                Deathmatch.Instance.SetRoundLength(minutes);
                Deathmatch.Instance.SetOverriden();
            }
        }

        public void SetDefaultValues() {
            if (Deathmatch.Instance != null) {
                Deathmatch.Instance.SetDefaultValues();
            }
        }

        public void SetPlaylist(AudioClip clip) => Internal_SetPlaylist(clip);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2) => Internal_SetPlaylist(clip1, clip2);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3) => Internal_SetPlaylist(clip1, clip2, clip3);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3, AudioClip clip4) => Internal_SetPlaylist(clip1, clip2, clip3, clip4);

        [HideFromIl2Cpp]
        private void Internal_SetPlaylist(params AudioClip[] clips) {
            if (Deathmatch.Instance != null) {
                Deathmatch.Instance.SetPlaylist(0.7f, clips);
                Deathmatch.Instance.SetOverriden();
            }
        }
#else
        public void StartGamemode() { }

        public void StopGamemode() { }

        public void SetDefaultValues() { }

        public void SetRoundLength(int minutes) { }

        public void SetPlaylist(AudioClip clip) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3, AudioClip clip4) { }
#endif
    }
}
