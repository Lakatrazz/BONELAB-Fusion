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
    public class Keyboard : MonoBehaviour
    {
#if MELONLOADER
        public Keyboard(IntPtr intPtr) : base(intPtr) { }

        public Action<string> OnValueChanged;

        public Action OnOpen, OnClose, OnEnter;

        private string _value = string.Empty;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;

                UpdateSettings();

                OnValueChanged?.Invoke(value);
            }
        }

        private bool _opened = false;
        public bool Opened => _opened;

        private bool _uppercase = false;
        private bool _temporaryUppercase = false;

        public bool TemporaryUppercase
        {
            get
            {
                return _temporaryUppercase;
            }
            set
            {
                _temporaryUppercase = value;
            }
        }

        public bool Uppercase
        {
            get
            {
                return _uppercase;
            }
            set
            {
                _uppercase = value;

                foreach (var button in _buttons)
                {
                    // Numeric characters shouldn't be affected by caps lock
                    if (!_temporaryUppercase && button.Special)
                    {
                        button.Uppercase = false;
                        continue;
                    }

                    button.Uppercase = value;
                }
            }
        }

        private KeyboardButton[] _buttons = null;

        private TMP_Text _textField = null;

        private void Awake()
        {
            // Cache text field
            _textField = transform.Find("label_TextField/text").GetComponent<TMP_Text>();

            // Cache buttons
            _buttons = GetComponentsInChildren<KeyboardButton>(true);

            foreach (var button in _buttons)
            {
                button.OnPressed += OnKeyPressed;
            }

            // Apply defaults
            UpdateSettings();
        }

        private void OnKeyPressed(string value)
        {
            Value += value;

            // If this is temporary uppercase (shift) then reset the state
            if (_temporaryUppercase && Uppercase)
            {
                Uppercase = false;
            }
        }

        public void UpdateSettings()
        {
            if (string.IsNullOrEmpty(Value))
            {
                _textField.color = Color.gray;

                _textField.text = "Enter text...";
            }
            else
            {
                _textField.color = Color.white;

                _textField.text = Value;
            }
        }

        public void Space()
        {
            Value += " ";
        }

        public void Open()
        {
            _opened = true;

            OnOpen?.Invoke();
        }

        public void Close()
        {
            _opened = false;

            OnClose?.Invoke();
        }

        public void Backspace()
        {
            // We can't remove characters if there aren't any left!
            if (Value.Length <= 0)
            {
                return;
            }

            Value = Value[..^1];
        }

        public void Shift()
        {
            bool newUppercase = !Uppercase;

            _temporaryUppercase = newUppercase;
            Uppercase = newUppercase;
        }

        public void Caps()
        {
            _temporaryUppercase = false;
            Uppercase = !Uppercase;
        }
        
        public void Enter()
        {
            OnEnter?.Invoke();

            Close();
        }

        public void Copy()
        {
            GUIUtility.systemCopyBuffer = Value;
        }
        
        public void Paste()
        {
            string clipboard = GUIUtility.systemCopyBuffer;

            if (!string.IsNullOrWhiteSpace(clipboard))
            {
                Value += clipboard;
            }
        }

        public void Clear()
        {
            Value = string.Empty;
        }
#else
        public void Space()
        {
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Backspace()
        {
        }

        public void Shift()
        {
        }

        public void Caps()
        {
        }

        public void Enter()
        {
        }

        public void Copy()
        {
        }

        public void Paste()
        {
        }

        public void Clear()
        {
        }
#endif
    }
}