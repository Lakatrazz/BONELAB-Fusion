﻿using SLZ.Marrow.Warehouse;
using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.SDK.Gamemodes;
using UnhollowerBaseLib.Attributes;
using LabFusion.Utilities;
#endif

namespace LabFusion.MarrowIntegration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Gamemodes/Team Deathmatch Proxy")]
    [DisallowMultipleComponent]
#endif
    public sealed class TeamDeathmatchProxy : FusionMarrowBehaviour
    {
#if MELONLOADER
        public TeamDeathmatchProxy(IntPtr intPtr) : base(intPtr) { }

        public void StartGamemode()
        {
            if (TeamDeathmatch.Instance != null)
            {
                TeamDeathmatch.Instance.StartGamemode(true);
            }
        }

        public void StopGamemode()
        {
            if (TeamDeathmatch.Instance != null)
            {
                TeamDeathmatch.Instance.StopGamemode();
            }
        }

        public void SetRoundLength(int minutes)
        {
            if (TeamDeathmatch.Instance != null)
            {
                TeamDeathmatch.Instance.SetOverriden();
                TeamDeathmatch.Instance.SetRoundLength(minutes);
            }
        }

        public void SetDefaultValues()
        {
            if (TeamDeathmatch.Instance != null)
            {
                TeamDeathmatch.Instance.SetDefaultValues();
            }
        }

        public void AddTeam(string Name, Color Color, Texture2D Logo, AvatarCrate Avatar)
        {
            TeamDeathmatch.Instance.SetOverriden();
            Team team = new Team(Name, Color);
            team.SetLogo(Logo);
            TeamDeathmatch.Instance.AddTeam(team);
        }

        public void SetAvatarOverride(string barcode)
        {
            if (TeamDeathmatch.Instance != null)
            {
                TeamDeathmatch.Instance?.SetOverriden();
                TeamDeathmatch.Instance.SetAvatarOverride(barcode);
            }
        }

        public void SetPlayerVitality(float vitality)
        {
            if (TeamDeathmatch.Instance != null)
            {
                TeamDeathmatch.Instance.SetOverriden();
                TeamDeathmatch.Instance.SetPlayerVitality(vitality);
            }
        }

        public void SetLateJoining(bool enabled)
        {
            if (TeamDeathmatch.Instance != null)
            {
                TeamDeathmatch.Instance.SetOverriden();
                TeamDeathmatch.Instance.SetLateJoining(enabled);
            }
        }

        public void SetTeamScore(int score) => Internal_SetTeamScore(null, score);
        public void IncrementTeamScore(int amount) => Internal_IncrementTeamScore(null, amount);

        public void SetSabrelakeScore(int score) => Internal_SetTeamScore(null, score);
        public void SetLavaGangScore(int score) => Internal_SetTeamScore(null, score);
        public void IncrementSabrelakeScore(int amount) => Internal_IncrementTeamScore(null, amount);
        public void IncrementLavaGangScore(int amount) => Internal_IncrementTeamScore(null, amount);

        private void Internal_SetTeamScore(Team team, int score)
        {
            if (TeamDeathmatch.Instance != null && TeamDeathmatch.Instance.IsActive())
            {
                TeamDeathmatch.Instance.SetScore(team, score);
            }
        }

        private void Internal_IncrementTeamScore(Team team, int amount)
        {
            if (TeamDeathmatch.Instance != null && TeamDeathmatch.Instance.IsActive())
            {
                TeamDeathmatch.Instance.SetScore(team, TeamDeathmatch.Instance.GetScoreFromTeam(team) + amount);
            }
        }

        public void SetPlaylist(AudioClip clip) => Internal_SetPlaylist(clip);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2) => Internal_SetPlaylist(clip1, clip2);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3) => Internal_SetPlaylist(clip1, clip2, clip3);
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3, AudioClip clip4) => Internal_SetPlaylist(clip1, clip2, clip3, clip4);

        [HideFromIl2Cpp]
        private void Internal_SetPlaylist(params AudioClip[] clips)
        {
            if (TeamDeathmatch.Instance != null)
            {
                TeamDeathmatch.Instance.SetOverriden();
                TeamDeathmatch.Instance.SetPlaylist(Gamemode.DefaultMusicVolume, clips);
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

        public void SetAvatarOverride(string barcode) { }

        public void SetPlayerVitality(float vitality) { }

        public void SetRoundLength(int minutes) { }

        public void AddTeam(string Name, Color Color, Texture2D Logo, AvatarCrate Avatar) { }

        public void SetLavaGangName(string name) { }

        public void SetSabrelakeName(string name) { }

        public void SetLavaGangLogo(Texture2D logo) { }

        public void SetSabrelakeLogo(Texture2D logo) { }


        public void SetSabrelakeScore(int score) { }
        public void SetLavaGangScore(int score) {  }
        public void IncrementSabrelakeScore(int amount) { }
        public void IncrementLavaGangScore(int amount) { }

        public void SetLateJoining(bool enabled) { }

        public void SetPlaylist(AudioClip clip) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3) { }
        public void SetPlaylist(AudioClip clip1, AudioClip clip2, AudioClip clip3, AudioClip clip4) { }
#endif
    }
}