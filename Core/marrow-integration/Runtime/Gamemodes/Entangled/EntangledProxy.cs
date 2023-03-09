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
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Entangled Proxy")]
    [DisallowMultipleComponent]
#endif
    public sealed class EntangledProxy : FusionMarrowBehaviour {
#if MELONLOADER
        public EntangledProxy(IntPtr intPtr) : base(intPtr) { }

        public void StartGamemode() {
            if (Entangled.Instance != null) {
                Entangled.Instance.StartGamemode(true);
            }
        }

        public void StopGamemode() {
            if (Entangled.Instance != null) {
                Entangled.Instance.StopGamemode();
            }
        }

        public void SetDefaultValues() {
            if (Entangled.Instance != null) {
                Entangled.Instance.SetDefaultValues();
            }
        }

        public void SetPlaylist(AudioClip clip) => Internal_SetPlaylist(clip);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2) => Internal_SetPlaylist(clip1, clip2);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3) => Internal_SetPlaylist(clip1, clip2, clip3);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3, AudioClip clip4) => Internal_SetPlaylist(clip1, clip2, clip3, clip4);

        [HideFromIl2Cpp]
        private void Internal_SetPlaylist(params AudioClip[] clips) {
            if (Entangled.Instance != null) {
                Entangled.Instance.SetOverriden();
                Entangled.Instance.SetPlaylist(Gamemode.DefaultMusicVolume, clips);
            }
        }
#else
        public override string Comment => "A proxy script for triggering and configuring Entangled in your map.\n" +
    "You can use UltEvents or UnityEvents to trigger these functions. (ex. LifeCycleEvent that calls SetPlaylist).";

        public void StartGamemode() { }

        public void StopGamemode() { }

        public void SetDefaultValues() { }

        public void SetPlaylist(AudioClip clip) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3, AudioClip clip4) { }

#endif
    }
}
