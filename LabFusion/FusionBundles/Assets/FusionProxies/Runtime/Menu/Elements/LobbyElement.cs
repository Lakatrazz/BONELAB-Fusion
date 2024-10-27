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
    public class LobbyElement : MenuElement
    {
#if MELONLOADER
        public LobbyElement(IntPtr intPtr) : base(intPtr) { }

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

        public RawImage LevelIcon { get; set; } = null;

        public MenuPage LobbyPage { get; set; } = null;

        public LabelElement LevelNameElement { get; set; } = null;
        public LabelElement ServerVersionElement { get; set; } = null;
        public IntElement PlayersElement { get; set; } = null;

        public FunctionElement ServerActionElement { get; set; } = null;
        public FunctionElement MoreElement { get; set; } = null;
        public EnumElement PrivacyElement { get; set; } = null;
        public StringElement ServerNameElement { get; set; } = null;
        public LabelElement HostNameElement { get; set; } = null;
        public StringElement DescriptionElement { get; set; } = null;

        public PageElement SettingsElement { get; set; } = null;

        public GameObject BansGrid { get; set; } = null;
        public PageElement BansElement { get; set; } = null;

        public GameObject AdminGrid { get; set; } = null;
        public PageElement AdminElement { get; set; } = null;

        public GameObject CodeGrid { get; set; } = null;
        public StringElement CodeElement { get; set; } = null;
        public FunctionElement CodeRefreshElement { get; set; } = null;

        public PageElement PlayerBrowserElement { get; set; } = null;
        public PlayerElement ProfileElement { get; set; } = null;

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

            LobbyPage = GetComponent<MenuPage>();

            // Main panel
            var mainPanel = transform.Find("panel_Main");

            var levelInfoGrid = mainPanel.Find("grid_LevelInfo");

            LevelIcon = levelInfoGrid.Find("background_Level/icon_Level").GetComponent<RawImage>();

            LevelNameElement = levelInfoGrid.Find("label_LevelName").GetComponent<LabelElement>();
            ServerVersionElement = levelInfoGrid.Find("label_ServerVersion").GetComponent<LabelElement>();
            PlayersElement = levelInfoGrid.Find("button_Players").GetComponent<IntElement>();
            PrivacyElement = levelInfoGrid.Find("button_Privacy").GetComponent<EnumElement>();
            ServerActionElement = levelInfoGrid.Find("button_ServerAction").GetComponent<FunctionElement>();
            MoreElement = levelInfoGrid.Find("button_More").GetComponent<FunctionElement>();

            var serverInfoGrid = mainPanel.Find("grid_ServerInfo");

            ServerNameElement = serverInfoGrid.Find("button_ServerName").GetComponent<StringElement>();
            HostNameElement = serverInfoGrid.Find("label_HostName").GetComponent<LabelElement>();
            DescriptionElement = serverInfoGrid.Find("button_Description").GetComponent<StringElement>();
            PlayerBrowserElement = serverInfoGrid.Find("scrollRect_PlayerBrowser/Viewport/Content").GetComponent<PageElement>();

            var codeGrid = serverInfoGrid.Find("grid_Code");

            CodeGrid = codeGrid.gameObject;
            CodeElement = codeGrid.Find("button_Code").GetComponent<StringElement>();
            CodeRefreshElement = codeGrid.Find("button_Refresh").GetComponent<FunctionElement>();

            // More panel
            var morePanel = transform.Find("panel_More");
            var groupsGrid = morePanel.Find("grid_Groups");

            var settingsGrid = groupsGrid.Find("grid_Settings");
            SettingsElement = settingsGrid.Find("scrollRect_Settings/Viewport/Content").GetComponent<PageElement>();

            var bansGrid = groupsGrid.Find("grid_Bans");

            BansGrid = bansGrid.gameObject;
            BansElement = bansGrid.Find("scrollRect_Bans/Viewport/Content").GetComponent<PageElement>();

            var adminGrid = morePanel.Find("grid_Admin");
            AdminGrid = adminGrid.gameObject;
            AdminElement = adminGrid.Find("scrollRect_Admin/Viewport/Content").GetComponent<PageElement>();

            // Profile panel
            var profilePanel = transform.Find("panel_Profile");
            ProfileElement = profilePanel.GetComponent<PlayerElement>();

            _hasElements = true;
        }

        protected override void OnDraw()
        {
            base.OnDraw();

            if (PlayersElement != null)
            {
                PlayersElement.Interactable = Interactable;
            }

            if (PrivacyElement != null)
            {
                PrivacyElement.Interactable = Interactable;
            }

            if (ServerNameElement != null)
            {
                ServerNameElement.Interactable = Interactable;
            }

            if (DescriptionElement != null)
            {
                DescriptionElement.Interactable = Interactable;
            }

            if (ProfileElement != null)
            {
                ProfileElement.Interactable = Interactable;
            }
        }
#endif
    }
}