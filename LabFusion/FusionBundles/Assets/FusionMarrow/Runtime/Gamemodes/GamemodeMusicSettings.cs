using UnityEngine;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class GamemodeMusicSettings : MonoBehaviour
    {
#if MELONLOADER
        public GamemodeMusicSettings(IntPtr intPtr) : base(intPtr) { }

        public static GamemodeMusicSettings Instance { get; set; } = null;

        private readonly Dictionary<string, string> _victorySongOverrides = new();
        public Dictionary<string, string> VictorySongOverrides => _victorySongOverrides;

        private readonly Dictionary<string, string> _failureSongOverrides = new();
        public Dictionary<string, string> FailureSongOverrides => _failureSongOverrides;

        private string _tieSongOverride = null;
        public string TieSongOverride => _tieSongOverride;

        private readonly HashSet<string> _songOverrides = new();
        public HashSet<string> SongOverrides => _songOverrides;

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
            _victorySongOverrides[teamBarcode] = monoDiscBarcode;
        }

        public void SetFailureSong(string teamBarcode, string monoDiscBarcode)
        {
            _failureSongOverrides[teamBarcode] = monoDiscBarcode;
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
            _victorySongOverrides.Clear();
            _failureSongOverrides.Clear();
            _tieSongOverride = null;
            _songOverrides.Clear();
        }
#else
        public void SetVictorySong(string teamBarcode, string monoDiscBarcode)
        {

        }

        public void SetFailureSong(string teamBarcode, string monoDiscBarcode)
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