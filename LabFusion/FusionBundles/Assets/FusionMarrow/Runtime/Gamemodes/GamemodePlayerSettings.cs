using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppInterop.Runtime.Attributes;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [HelpURL("https://github.com/Lakatrazz/BONELAB-Fusion/wiki/Gamemode-Maps#gamemode-player-settings")]
#endif
    public class GamemodePlayerSettings : MonoBehaviour
    {
#if MELONLOADER
        public GamemodePlayerSettings(IntPtr intPtr) : base(intPtr) { }

        public static GamemodePlayerSettings Instance { get; set; } = null;

        private string _avatarOverride = null;

        [HideFromIl2Cpp]
        public string AvatarOverride => _avatarOverride;

        private float? _vitalityOverride = null;

        [HideFromIl2Cpp]
        public float? VitalityOverride => _vitalityOverride;

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

        public void SetAvatar(string avatarBarcode)
        {
            _avatarOverride = avatarBarcode;
        }

        public void SetVitality(float vitality)
        {
            _vitalityOverride = vitality;
        }
#else
        public string avatarOverride = null;

        [Min(0f)]
        public float vitalityOverride = 0f;

        public void SetAvatar(string avatarBarcode)
        {
        }

        public void SetVitality(float vitality)
        {
        }
#endif
    }
}