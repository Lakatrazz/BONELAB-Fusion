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
    public class PlayerResultElement : MenuElement
    {
#if MELONLOADER
        public PlayerResultElement(IntPtr intPtr) : base(intPtr) { }

        public TMP_Text PlayerNameText { get; private set; } = null;
        public TMP_Text RoleText { get; private set; } = null;

        public RawImage UserIcon { get; private set; } = null;
        public RawImage UserStatus { get; private set; } = null;
        public RawImage UserBorder { get; private set; } = null;

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

            // Icons
            var iconRoot = transform.Find("label_ProfileIcon");

            UserIcon = iconRoot.Find("icon_Player").GetComponent<RawImage>();
            UserStatus = iconRoot.Find("icon_StatusDrop/icon_Status").GetComponent<RawImage>();
            UserBorder = iconRoot.Find("border").GetComponent<RawImage>();

            // Info layout
            var infoLayout = transform.Find("layout_Info");

            PlayerNameText = infoLayout.Find("text_PlayerName").GetComponent<TMP_Text>();
            RoleText = infoLayout.Find("text_Role").GetComponent<TMP_Text>();

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