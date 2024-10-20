#if MELONLOADER
using MelonLoader;
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

        public LabelElement LevelNameElement { get; set; } = null;
        public LabelElement ServerVersionElement { get; set; } = null;
        public IntElement PlayersElement { get; set; } = null;

        public FunctionElement ServerActionElement { get; set; } = null;
        public EnumElement PrivacyElement { get; set; } = null;
        public StringElement ServerNameElement { get; set; } = null;
        public LabelElement HostNameElement { get; set; } = null;
        public StringElement DescriptionElement { get; set; } = null;

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

            LevelIcon = levelInfoGrid.Find("background_Level/icon_Level").GetComponent<RawImage>();

            LevelNameElement = levelInfoGrid.Find("label_LevelName").GetComponent<LabelElement>();
            ServerVersionElement = levelInfoGrid.Find("label_ServerVersion").GetComponent<LabelElement>();
            PlayersElement = levelInfoGrid.Find("button_Players").GetComponent<IntElement>();
            PrivacyElement = levelInfoGrid.Find("button_Privacy").GetComponent<EnumElement>();
            ServerActionElement = levelInfoGrid.Find("button_ServerAction").GetComponent<FunctionElement>();

            var serverInfoGrid = transform.Find("grid_ServerInfo");

            ServerNameElement = serverInfoGrid.Find("button_ServerName").GetComponent<StringElement>();
            HostNameElement = serverInfoGrid.Find("label_HostName").GetComponent<LabelElement>();
            DescriptionElement = serverInfoGrid.Find("button_Description").GetComponent<StringElement>();

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
        }
#endif
    }
}