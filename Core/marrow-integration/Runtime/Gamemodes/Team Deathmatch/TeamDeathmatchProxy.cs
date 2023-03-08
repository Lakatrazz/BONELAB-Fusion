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
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Team Deathmatch Proxy")]
    [DisallowMultipleComponent]
#endif
    public sealed class TeamDeathmatchProxy : FusionMarrowBehaviour {
#if MELONLOADER
        public TeamDeathmatchProxy(IntPtr intPtr) : base(intPtr) { }

        public void StartGamemode() {
            if (TeamDeathmatch.Instance != null) {
                TeamDeathmatch.Instance.StartGamemode(true);
            }
        }

        public void StopGamemode() {
            if (TeamDeathmatch.Instance != null) {
                TeamDeathmatch.Instance.StopGamemode();
            }
        }

        public void SetRoundLength(int minutes) {
            if (TeamDeathmatch.Instance != null) {
                TeamDeathmatch.Instance.SetRoundLength(minutes);
                TeamDeathmatch.Instance.SetOverriden();
            }
        }

        public void SetDefaultValues() {
            if (TeamDeathmatch.Instance != null) {
                TeamDeathmatch.Instance.SetDefaultValues();
            }
        }

        public void SetLavaGangName(string name) {
            TeamDeathmatch.Instance?.SetLavaGangName(name);
            TeamDeathmatch.Instance?.SetOverriden();
        }

        public void SetSabrelakeName(string name) {
            TeamDeathmatch.Instance?.SetSabrelakeName(name);
            TeamDeathmatch.Instance?.SetOverriden();
        }

        public void SetLavaGangLogo(Texture2D logo) {
            TeamDeathmatch.Instance?.SetLavaGangLogo(logo);
            TeamDeathmatch.Instance?.SetOverriden();
        }

        public void SetSabrelakeLogo(Texture2D logo) {
            TeamDeathmatch.Instance?.SetSabrelakeLogo(logo);
            TeamDeathmatch.Instance?.SetOverriden();
        }

        public void SetPlaylist(AudioClip clip) => Internal_SetPlaylist(clip);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2) => Internal_SetPlaylist(clip1, clip2);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3) => Internal_SetPlaylist(clip1, clip2, clip3);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3, AudioClip clip4) => Internal_SetPlaylist(clip1, clip2, clip3, clip4);

        [HideFromIl2Cpp]
        private void Internal_SetPlaylist(params AudioClip[] clips) {
            if (TeamDeathmatch.Instance != null) {
                TeamDeathmatch.Instance.SetPlaylist(Gamemode.DefaultMusicVolume, clips);
                TeamDeathmatch.Instance.SetOverriden();
            }
        }
#else
        public override string Comment => "A proxy script for triggering and configuring Team Deathmatch in your map.\n" +
            "You can use UltEvents or UnityEvents to trigger these functions. (ex. LifeCycleEvent that calls SetRoundLength).\n" +
            "Most settings can be configured, such as round length, team names, logos, etc.\n" +
            "The gamemode can also be started and stopped from here.";

        public void StartGamemode() { }

        public void StopGamemode() { }

        public void SetDefaultValues() { }

        public void SetRoundLength(int minutes) { }

        public void SetLavaGangName(string name) { }

        public void SetSabrelakeName(string name) { }

        public void SetLavaGangLogo(Texture2D logo) { }

        public void SetSabrelakeLogo(Texture2D logo) { }

        public void SetPlaylist(AudioClip clip) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3, AudioClip clip4) { }
#endif
    }
}
