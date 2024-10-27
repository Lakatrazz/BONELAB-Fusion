#if MELONLOADER
using Il2CppTMPro;
using MelonLoader;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class KeyboardButton : MonoBehaviour
    {
#if MELONLOADER
        public event Action<string> OnPressed;

        public KeyboardButton(IntPtr intPtr) : base(intPtr) { }

        private string _lowercaseKey = null;
        private string _uppercaseKey = null;

        public bool Special { get; set; } = false;

        private bool _uppercase = false;

        public bool Uppercase
        {
            get
            {
                return _uppercase;
            }
            set
            {
                _uppercase = value;

                UpdateSettings();
            }
        }

        public string Key
        {
            get
            {
                if (_uppercase && !string.IsNullOrWhiteSpace(_uppercaseKey))
                {
                    return _uppercaseKey;
                }

                return _lowercaseKey;
            }
        }

        private TMP_Text _keyText = null;

        private void Awake()
        {
            // Get key text reference
            _keyText = transform.Find("text").GetComponent<TMP_Text>();

            // Get default keys
            var variablesRoot = transform.Find("Key Variables");

            if (variablesRoot != null)
            {
                _lowercaseKey = variablesRoot.GetChild(0).name;

                if (variablesRoot.childCount > 1)
                {
                    _uppercaseKey = variablesRoot.GetChild(1).name;
                }
            }

            // Check if this is a special key
            var specialTransform = variablesRoot.Find("Special");

            if (specialTransform != null)
            {
                Special = true;
            }

            // Set text value
            UpdateSettings();
        }

        public void UpdateSettings()
        {
            _keyText.text = Key;
        }

        public void Press()
        {
            OnPressed?.Invoke(Key);
        }
#else
        public void Press()
        {

        }
#endif
    }
}