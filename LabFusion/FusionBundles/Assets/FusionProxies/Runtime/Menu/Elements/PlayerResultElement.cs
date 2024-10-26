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

        public RawImage PlayerIcon { get; private set; } = null;
        public RawImage PlayerStatus { get; private set; } = null;
        public RawImage PlayerBorder { get; private set; } = null;

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

            PlayerIcon = iconRoot.Find("icon_Mask/icon_Player").GetComponent<RawImage>();
            PlayerStatus = iconRoot.Find("icon_StatusDrop/icon_Status").GetComponent<RawImage>();
            PlayerBorder = iconRoot.Find("border").GetComponent<RawImage>();

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