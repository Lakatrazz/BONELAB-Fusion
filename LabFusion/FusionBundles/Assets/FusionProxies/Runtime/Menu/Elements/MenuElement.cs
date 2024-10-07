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
    public class MenuElement : MonoBehaviour
    {
#if MELONLOADER
        public MenuElement(IntPtr intPtr) : base(intPtr) { }

        private string _title = "Element";

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

        private Color _color = Color.white;

        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;

                UpdateSettings();
            }
        }

        public TMP_Text Text { get { return _text; } set { _text = value; } }

        private TMP_Text _text = null;

        public event Action OnDestroyed;

        protected virtual void Awake()
        {
            var textTransform = transform.Find("text");

            if (textTransform != null)
            {
                _text = textTransform.GetComponent<TMP_Text>();
            }
        }

        protected virtual void OnEnable()
        {
            UpdateSettings();
        }

        protected virtual void OnDestroy()
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
                Text.color = _color;
            }
        }
#endif
    }
}