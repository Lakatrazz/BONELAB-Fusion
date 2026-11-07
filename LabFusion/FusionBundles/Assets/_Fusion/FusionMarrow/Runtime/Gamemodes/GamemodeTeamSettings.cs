using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppInterop.Runtime.Attributes;
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
    [HelpURL("https://github.com/Lakatrazz/BONELAB-Fusion/wiki/Gamemode-Maps#gamemode-team-settings")]
#endif
    public class GamemodeTeamSettings : MonoBehaviour
    {
#if MELONLOADER
        public GamemodeTeamSettings(IntPtr intPtr) : base(intPtr) { }

        public static GamemodeTeamSettings Instance { get; set; } = null;

        private readonly Dictionary<string, string> _teamNameOverrides = new();

        [HideFromIl2Cpp]
        public Dictionary<string, string> TeamNameOverrides => _teamNameOverrides;

        private readonly Dictionary<string, Texture2D> _teamLogoOverrides = new();

        [HideFromIl2Cpp]
        public Dictionary<string, Texture2D> TeamLogoOverrides => _teamLogoOverrides;

        [HideFromIl2Cpp]
        public void ApplyOverrides(string barcode, ref string displayName, ref Texture logo)
        {
            if (TeamNameOverrides.TryGetValue(barcode, out var nameOverride))
            {
                displayName = nameOverride;
            }

            if (TeamLogoOverrides.TryGetValue(barcode, out var logoOverride))
            {
                logo = logoOverride;
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

        public void SetTeamName(string barcode, string name)
        {
            _teamNameOverrides[barcode] = name;
        }

        public void SetTeamLogo(string barcode, Texture2D logo)
        {
            _teamLogoOverrides[barcode] = logo;
        }
#else
        [Serializable]
        public struct TeamOverride
        {
            public BoneTagReference teamTag;

            public string overrideName;

            public Texture2D overrideLogo;
        }

        public List<TeamOverride> teamOverrides = new();

        public void SetTeamName(string barcode, string name)
        {

        }

        public void SetTeamLogo(string barcode, Texture2D logo)
        {

        }
#endif
    }
}