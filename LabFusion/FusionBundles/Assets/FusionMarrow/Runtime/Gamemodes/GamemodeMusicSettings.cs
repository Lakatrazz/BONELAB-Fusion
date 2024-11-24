using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppInterop.Runtime.Attributes;
#else
using System;
using System.Collections.Generic;
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

        private readonly HashSet<string> _songOverrides = new();

        [HideFromIl2Cpp]
        public HashSet<string> SongOverrides => _songOverrides;

        private string _victorySongOverride = null;

        [HideFromIl2Cpp]
        public string VictorySongOverride => _victorySongOverride;

        private string _failureSongOverride = null;

        [HideFromIl2Cpp]
        public string FailureSongOverride => _failureSongOverride;

        private string _tieSongOverride = null;

        [HideFromIl2Cpp]
        public string TieSongOverride => _tieSongOverride;

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
            public string teamBarcode;

            public string victorySongOverride;
            public string failureSongOverride;
        }

        public List<TeamOverride> teamOverrides = new();

        public List<string> songOverrides = new();

        public string victorySongOverride = null;

        public string failureSongOverride = null;

        public string tieSongOverride = null;

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