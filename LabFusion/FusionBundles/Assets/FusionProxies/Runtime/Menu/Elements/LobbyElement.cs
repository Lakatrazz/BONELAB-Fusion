#if MELONLOADER
using MelonLoader;
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

        public LabelElement LevelNameElement { get; set; } = null;
        public LabelElement ServerVersionElement { get; set; } = null;
        public LabelElement PlayerCountElement { get; set; } = null;
        public IntElement MaxPlayersElement { get; set; } = null;

        public FunctionElement ServerActionElement { get; set; } = null;
        public EnumElement PrivacyElement { get; set; } = null;
        public StringElement ServerNameElement { get; set; } = null;
        public LabelElement HostNameElement { get; set; } = null;

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

            var levelInfoGrid = transform.Find("grid_LevelInfo");

            LevelNameElement = levelInfoGrid.Find("label_LevelName").GetComponent<LabelElement>();
            ServerVersionElement = levelInfoGrid.Find("label_ServerVersion").GetComponent<LabelElement>();
            PlayerCountElement = levelInfoGrid.Find("label_PlayerCount").GetComponent<LabelElement>();
            MaxPlayersElement = levelInfoGrid.Find("button_MaxPlayers").GetComponent<IntElement>();

            var serverInfoGrid = transform.Find("grid_ServerInfo");

            ServerActionElement = serverInfoGrid.Find("button_ServerAction").GetComponent<FunctionElement>();
            PrivacyElement = serverInfoGrid.Find("button_Privacy").GetComponent<EnumElement>();
            ServerNameElement = serverInfoGrid.Find("button_ServerName").GetComponent<StringElement>();
            HostNameElement = serverInfoGrid.Find("label_HostName").GetComponent<LabelElement>();

            _hasElements = true;
        }

        protected override void OnDraw()
        {
            base.OnDraw();

            if (MaxPlayersElement != null)
            {
                MaxPlayersElement.Interactable = Interactable;
            }

            if (PrivacyElement != null)
            {
                PrivacyElement.Interactable = Interactable;
            }

            if (ServerNameElement != null)
            {
                ServerNameElement.Interactable = Interactable;
            }
        }
#endif
    }
}