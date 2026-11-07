#if MELONLOADER
using Il2CppTMPro;

using MelonLoader;

using UnityEngine.UI;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class LevelDownloadUI : MonoBehaviour
    {
#if MELONLOADER
        public LevelDownloadUI(IntPtr intPtr) : base(intPtr) { }

        public static LevelDownloadUI Instance { get; private set; } = null;

        public TMP_Text LevelTitleText { get; set; } = null;

        public Slider ProgressBarSlider { get; set; } = null;
        public TMP_Text ProgressBarText { get; set; } = null;

        public RawImage LevelIcon { get; set; } = null;

        private bool _hasReferences = false;

        private void Awake()
        {
            Instance = this;

            GetReferences();
        }

        private void GetReferences()
        {
            if (_hasReferences)
            {
                return;
            }

            LevelTitleText = transform.Find("Downloading/Level Name").GetComponent<TMP_Text>();

            ProgressBarSlider = transform.Find("Downloading/Progress Bar").GetComponent<Slider>();
            ProgressBarText = ProgressBarSlider.transform.Find("text").GetComponent<TMP_Text>();

            LevelIcon = transform.Find("Level Icon").GetComponent<RawImage>();

            _hasReferences = true;
        }
#endif
    }
}