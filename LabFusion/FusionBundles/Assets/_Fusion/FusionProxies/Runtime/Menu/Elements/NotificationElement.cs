#if MELONLOADER
using Il2CppTMPro;

using MelonLoader;

using UnityEngine;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class NotificationElement : MenuElement
    {
#if MELONLOADER
        public NotificationElement(IntPtr intPtr) : base(intPtr) { }

        public TMP_Text TitleText { get; set; } = null;
        public TMP_Text MessageText { get; set; } = null;

        public GameObject AcceptButton { get; set; } = null;
        public GameObject DeclineButton { get; set; } = null;

        public Action OnAccepted, OnDeclined;

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

            // Info layout
            var infoLayout = transform.Find("layout_Info");

            TitleText = infoLayout.Find("text_Title").GetComponent<TMP_Text>();
            MessageText = infoLayout.Find("text_Message").GetComponent<TMP_Text>();

            // Options layout
            var optionsLayout = transform.Find("layout_Options");

            AcceptButton = optionsLayout.Find("button_Accept").gameObject;
            DeclineButton = optionsLayout.Find("button_Decline").gameObject;

            _hasReferences = true;
        }

        public void Accept()
        {
            OnAccepted?.Invoke();
        }

        public void Decline()
        {
            OnDeclined?.Invoke();
        }
#else
        public void Accept()
        {
        }

        public void Decline()
        {
        }
#endif
    }
}