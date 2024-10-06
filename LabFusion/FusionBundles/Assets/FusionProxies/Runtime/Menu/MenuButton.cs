using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppTMPro;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class MenuButton : MonoBehaviour
    {
#if MELONLOADER
        public MenuButton(IntPtr intPtr) : base(intPtr) { }

        private string _title = "Button";

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;

                UpdateSettings();
            }
        }

        public TMP_Text Text => _text;

        private TMP_Text _text = null;

        public event Action OnDestroyed;

        protected virtual void Awake()
        {
            var textTransform = transform.Find("text");

            if (textTransform != null)
            {
                _text = textTransform.GetComponent<TMP_Text>();
            }

            UpdateSettings();
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
            OnDestroyed = null;
        }

        public void UpdateSettings()
        {
            UpdateText();
        }

        public virtual void UpdateText()
        {
            if (Text != null)
            {
                Text.text = _title;
            }
        }
#endif
    }
}