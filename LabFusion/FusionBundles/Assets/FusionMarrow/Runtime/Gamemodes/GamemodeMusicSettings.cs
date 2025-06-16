using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppInterop.Runtime.Attributes;

using Il2CppSLZ.Marrow.Warehouse;
#else
using System;
using System.Collections.Generic;

using SLZ.Marrow.Warehouse;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/Lakatrazz/BONELAB-Fusion/wiki/Gamemode-Maps#gamemode-music-settings")]
#endif
    public class GamemodeMusicSettings : MonoBehaviour
    {
#if MELONLOADER
        public GamemodeMusicSettings(IntPtr intPtr) : base(intPtr) { }

        public static GamemodeMusicSettings Instance { get; set; } = null;

        private readonly Dictionary<string, string> _teamVictorySongOverrides = new();

        [HideFromIl2Cpp]
        public Dictionary<string, string> TeamVictorySongOverrides => _teamVictorySongOverrides;

        private readonly Dictionary<string, string> _teamFailureSongOverrides = new();

        [HideFromIl2Cpp]
        public Dictionary<string, string> TeamFailureSongOverrides => _teamFailureSongOverrides;

        private readonly List<string> _songOverrides = new();

        [HideFromIl2Cpp]
        public List<string> SongOverrides => _songOverrides;

        private string _victorySongOverride = null;

        [HideFromIl2Cpp]
        public string VictorySongOverride => _victorySongOverride;

        private string _failureSongOverride = null;

        [HideFromIl2Cpp]
        public string FailureSongOverride => _failureSongOverride;

        private string _tieSongOverride = null;

        [HideFromIl2Cpp]
        public string TieSongOverride => _tieSongOverride;

        [HideFromIl2Cpp]
        public void ApplyTeamOverrides(string barcode, ref MonoDiscReference victorySong, ref MonoDiscReference failureSong)
        {
            if (TeamVictorySongOverrides.TryGetValue(barcode, out var victorySongOverride))
            {
                victorySong = new(victorySongOverride);
            }

            if (TeamFailureSongOverrides.TryGetValue(barcode, out var failureSongOverride))
            {
                failureSong = new(failureSongOverride);
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetVictorySong(string teamBarcode, string monoDiscBarcode)
        {
            _teamVictorySongOverrides[teamBarcode] = monoDiscBarcode;
        }

        public void SetVictorySong(string monoDiscBarcode)
        {
            _victorySongOverride = monoDiscBarcode;
        }

        public void SetFailureSong(string teamBarcode, string monoDiscBarcode)
        {
            _teamFailureSongOverrides[teamBarcode] = monoDiscBarcode;
        }

        public void SetFailureSong(string monoDiscBarcode)
        {
            _failureSongOverride = monoDiscBarcode;
        }

        public void SetTieSong(string monoDiscBarcode)
        {
            _tieSongOverride = monoDiscBarcode;
        }

        public void AddSong(string monoDiscBarcode)
        {
            _songOverrides.Add(monoDiscBarcode);
        }

        public void RemoveSong(string monoDiscBarcode)
        {
            _songOverrides.Remove(monoDiscBarcode);
        }

        public void ClearOverrides()
        {
            _teamVictorySongOverrides.Clear();
            _teamFailureSongOverrides.Clear();

            _songOverrides.Clear();

            _victorySongOverride = null;
            _failureSongOverride = null;
            _tieSongOverride = null;
        }
#else
        [Serializable]
        public struct TeamOverride
        {
            public BoneTagReference teamTag;

            public MonoDiscReference victorySongOverride;
            public MonoDiscReference failureSongOverride;
        }

        public List<TeamOverride> teamOverrides = new();

        public List<MonoDiscReference> songOverrides = new();

        public MonoDiscReference victorySongOverride = null;

        public MonoDiscReference failureSongOverride = null;

        public MonoDiscReference tieSongOverride = null;

        public void SetVictorySong(string teamBarcode, string monoDiscBarcode)
        {

        }

        public void SetVictorySong(string monoDiscBarcode)
        {

        }

        public void SetFailureSong(string teamBarcode, string monoDiscBarcode)
        {

        }

        public void SetFailureSong(string monoDiscBarcode)
        {

        }

        public void SetTieSong(string monoDiscBarcode)
        {

        }

        public void AddSong(string monoDiscBarcode)
        {

        }

        public void RemoveSong(string monoDiscBarcode)
        {

        }

        public void ClearOverrides()
        {

        }
#endif
    }
}