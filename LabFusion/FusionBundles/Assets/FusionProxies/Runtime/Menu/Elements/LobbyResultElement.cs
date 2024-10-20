#if MELONLOADER
using Il2CppTMPro;

using MelonLoader;

using UnityEngine.UI;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class LobbyResultElement : MenuElement
    {
#if MELONLOADER
        public LobbyResultElement(IntPtr intPtr) : base(intPtr) { }

        public TMP_Text LevelNameText { get; private set; } = null;

        public TMP_Text ServerNameText { get; private set; } = null;
        public TMP_Text HostNameText { get; private set; } = null;
        public TMP_Text PlayerCountText { get; private set; } = null;
        public TMP_Text VersionText { get; private set; } = null;

        public RawImage LevelIcon { get; private set; } = null;

        public Action OnPressed;

        private bool _hasReferences = false;

        public void Awake()
        {
            GetReferences();
        }
        
        public void GetReferences()
        {
            if (_hasReferences) 
            { 
                return; 
            }

            // Level name
            var levelNameBackground = transform.Find("levelName_Background");

            LevelNameText = levelNameBackground.Find("text_LevelName").GetComponent<TMP_Text>();

            // Icons
            var iconRoot = transform.Find("icon_Background");

            LevelIcon = iconRoot.Find("icon_Level").GetComponent<RawImage>();

            // Info layout
            var infoLayout = transform.Find("layout_Info");

            ServerNameText = infoLayout.Find("text_ServerName").GetComponent<TMP_Text>();
            HostNameText = infoLayout.Find("text_HostName").GetComponent<TMP_Text>();
            PlayerCountText = infoLayout.Find("text_PlayerCount").GetComponent<TMP_Text>();
            VersionText = infoLayout.Find("text_Version").GetComponent<TMP_Text>();

            _hasReferences = true;
        }

        public void Press()
        {
            OnPressed?.Invoke();
        }
#else
        public void Press()
        {

        }
#endif
    }
}