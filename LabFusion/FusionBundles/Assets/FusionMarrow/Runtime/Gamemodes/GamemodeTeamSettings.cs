using UnityEngine;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class GamemodeTeamSettings : MonoBehaviour
    {
#if MELONLOADER
        public GamemodeTeamSettings(IntPtr intPtr) : base(intPtr) { }

        public static GamemodeTeamSettings Instance { get; set; } = null;

        private readonly Dictionary<string, string> _teamNameOverrides = new();
        public Dictionary<string, string> TeamNameOverrides => _teamNameOverrides;

        private readonly Dictionary<string, Texture2D> _teamLogoOverrides = new();
        public Dictionary<string, Texture2D> TeamLogoOverrides => _teamLogoOverrides;

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

        public void SetTeamName(string barcode, string name)
        {
            _teamNameOverrides[barcode] = name;
        }

        public void SetTeamLogo(string barcode, Texture2D logo)
        {
            _teamLogoOverrides[barcode] = logo;
        }
#else
        public void SetTeamName(string barcode, string name)
        {

        }

        public void SetTeamLogo(string barcode, Texture2D logo)
        {

        }
#endif
    }
}