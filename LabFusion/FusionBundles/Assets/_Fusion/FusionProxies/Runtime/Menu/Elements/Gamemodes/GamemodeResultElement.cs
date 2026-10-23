#if MELONLOADER
using Il2CppTMPro;

using MelonLoader;

using UnityEngine;
using UnityEngine.UI;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class GamemodeResultElement : MenuElement
    {
#if MELONLOADER
        public GamemodeResultElement(IntPtr intPtr) : base(intPtr) { }

        public TMP_Text GamemodeNameText { get; private set; } = null;

        public GameObject BorderGameObject { get; private set; } = null;

        public RawImage GamemodeIcon { get; private set; } = null;

        public Action OnPressed;

        private bool _hasReferences = false;

        public void Awake()
        {
            GetReferences();
        }
        
        public void Highlight(bool highlighted)
        {
            GetReferences();

            BorderGameObject.SetActive(highlighted);
        }

        public void GetReferences()
        {
            if (_hasReferences) 
            { 
                return; 
            }

            // Gamemode name
            GamemodeNameText = transform.Find("text_GamemodeName").GetComponent<TMP_Text>();

            // Icons
            var iconRoot = transform.Find("label_GamemodeIcon");

            GamemodeIcon = iconRoot.Find("icon_Mask/icon_Gamemode").GetComponent<RawImage>();

            BorderGameObject = iconRoot.Find("icon_Border").gameObject;

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