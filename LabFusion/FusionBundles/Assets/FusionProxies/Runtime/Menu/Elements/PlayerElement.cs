#if MELONLOADER
using MelonLoader;

using UnityEngine;
using UnityEngine.UI;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class PlayerElement : MenuElement
    {
#if MELONLOADER
        public PlayerElement(IntPtr intPtr) : base(intPtr) { }

        private bool _interactable = true;
        public bool Interactable
        {
            get
            {
                return _interactable;
            }
            set
            {
                _interactable = value;

                Draw();
            }
        }

        public MenuPage PlayerPage { get; set; } = null;

        public RawImage PlayerIcon { get; set; } = null;
        public AspectRatioFitter PlayerIconFitter { get; set; } = null;

        public EnumElement PermissionsElement { get; set; } = null;

        public LabelElement UsernameElement { get; set; } = null;
        public StringElement NicknameElement { get; set; } = null;
        public StringElement DescriptionElement { get; set; } = null;

        public GameObject ActionsGrid { get; set; } = null;
        public PageElement ActionsElement { get; set; } = null;

        private bool _hasElements = false;

        public void Awake()
        {
            GetElements();
        }
        
        public void GetElements()
        {
            if (_hasElements) 
            { 
                return; 
            }

            PlayerPage = GetComponent<MenuPage>();

            // Main panel
            var mainPanel = transform.Find("panel_Main");

            var optionsGrid = mainPanel.Find("grid_PlayerOptions");

            PlayerIcon = optionsGrid.Find("label_ProfileIcon/icon_Mask/icon_Player").GetComponent<RawImage>();
            PlayerIconFitter = PlayerIcon.GetComponent<AspectRatioFitter>();

            PermissionsElement = optionsGrid.Find("button_Permissions").GetComponent<EnumElement>();

            var infoGrid = mainPanel.Find("grid_PlayerInfo");

            UsernameElement = infoGrid.Find("label_Username").GetComponent<LabelElement>();
            NicknameElement = infoGrid.Find("button_Nickname").GetComponent<StringElement>();
            DescriptionElement = infoGrid.Find("button_Description").GetComponent<StringElement>();

            var actionsGrid = infoGrid.Find("grid_Actions");
            ActionsGrid = actionsGrid.gameObject;

            ActionsElement = actionsGrid.Find("scrollRect_Actions/Viewport/Content").GetComponent<PageElement>();

            _hasElements = true;
        }

        protected override void OnDraw()
        {
            base.OnDraw();

            if (NicknameElement != null)
            {
                NicknameElement.Interactable = Interactable;
            }

            if (DescriptionElement != null)
            {
                DescriptionElement.Interactable = Interactable;
            }
        }
#endif
    }
}