#if MELONLOADER
using Il2CppTMPro;

using LabFusion.Scene;

using MelonLoader;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class LevelNameLabel : MonoBehaviour
    {
#if MELONLOADER
        public LevelNameLabel(IntPtr intPtr) : base(intPtr) { }

        private TMP_Text _text = null;

        private void Awake()
        {
            _text = GetComponentInChildren<TMP_Text>();

            _text.text = FusionSceneManager.Title;
        }
#endif
    }
}