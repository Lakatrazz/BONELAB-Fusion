using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.SDK.Gamemodes;
using UnhollowerBaseLib.Attributes;
using LabFusion.Utilities;
using SLZ.Marrow.Warehouse;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Deathmatch Proxy")]
    [DisallowMultipleComponent]
#endif
    public sealed class DeathmatchProxy : FusionMarrowBehaviour {
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
                Deathmatch.Instance.SetOverriden();
                Deathmatch.Instance.SetRoundLength(minutes);
            }
        }

        public void SetDefaultValues() {
            if (Deathmatch.Instance != null) {
                Deathmatch.Instance.SetDefaultValues();
            }
        }

        public void SetAvatarOverride(string barcode) {
            if (Deathmatch.Instance != null) {
                Deathmatch.Instance.SetOverriden();
                Deathmatch.Instance.SetAvatarOverride(barcode);
            }
        }

        public void SetPlayerVitality(float vitality)
        {
            if (Deathmatch.Instance != null)
            {
                Deathmatch.Instance.SetOverriden();
                Deathmatch.Instance.SetPlayerVitality(vitality);
            }
        }

        public void SetPlaylist(AudioClip clip) => Internal_SetPlaylist(clip);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2) => Internal_SetPlaylist(clip1, clip2);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3) => Internal_SetPlaylist(clip1, clip2, clip3);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3, AudioClip clip4) => Internal_SetPlaylist(clip1, clip2, clip3, clip4);

        [HideFromIl2Cpp]
        private void Internal_SetPlaylist(params AudioClip[] clips) {
            if (Deathmatch.Instance != null) {
                Deathmatch.Instance.SetOverriden();
                Deathmatch.Instance.SetPlaylist(Gamemode.DefaultMusicVolume, clips);
            }
        }
#else
        public override string Comment => "A proxy script for triggering and configuring Deathmatch in your map.\n" +
    "You can use UltEvents or UnityEvents to trigger these functions. (ex. LifeCycleEvent that calls SetRoundLength).\n" +
    "Most settings can be configured, such as round length, music, etc.\n" +
    "The gamemode can also be started and stopped from here.";

        public void StartGamemode() { }

        public void StopGamemode() { }

        public void SetDefaultValues() { }

        public void SetAvatarOverride(string barcode) { }

        public void SetPlayerVitality(float vitality) { }

        public void SetRoundLength(int minutes) { }

        public void SetPlaylist(AudioClip clip) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3, AudioClip clip4) { }
#endif
    }
}
