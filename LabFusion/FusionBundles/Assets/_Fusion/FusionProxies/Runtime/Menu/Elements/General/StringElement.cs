#if MELONLOADER
using LabFusion.Menu;

using MelonLoader;

using UnityEngine;
using UnityEngine.UI;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class StringElement : ButtonElement
    {
#if MELONLOADER
        public StringElement(IntPtr intPtr) : base(intPtr) { }

        public Action<string> OnValueChanged;
        public Action OnSubmitted;
        public Action<bool> OnKeyboardToggled;

        private string _value = null;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;

                Draw();

                OnValueChanged?.Invoke(value);
            }
        }

        private string _emptyFormat = "Click to add {0}...";
        public string EmptyFormat
        {
            get
            {
                return _emptyFormat;
            }
            set
            {
                _emptyFormat = value;

                Draw();
            }
        }

        public override string DefaultTextFormat => "{0}: {1}";

        private Button _button = null;

        protected override void Awake()
        {
            base.Awake();

            _button = GetComponent<Button>();
        }

        protected override void OnDraw()
        {
            base.OnDraw();

            if (_button != null)
            {
                _button.interactable = Interactable;
            }
        }

        public void Submit()
        {
            OnSubmitted?.Invoke();
        }

        public void ToggleKeyboard()
        {
            if (MenuKeyboardHelper.KeyboardOpened)
            {
                MenuKeyboardHelper.CloseKeyboard();

                OnKeyboardToggled?.Invoke(false);
            }
            else
            {
                MenuKeyboardHelper.AssignKeyboardToButton(this);

                OnKeyboardToggled?.Invoke(true);
            }
        }

        public override void UpdateText()
        {
            if (Text != null)
            {
                if (string.IsNullOrEmpty(Value))
                {
                    Text.text = string.Format(EmptyFormat, Title.ToLower());
                    Text.color = Color.gray * Color;
                }
                else
                {
                    Text.text = string.Format(TextFormat, Title, Value);
                    Text.color = Color;
                }
            }
        }

        protected override void OnClearValues()
        {
            _value = null;
            OnValueChanged = null;

            base.OnClearValues();
        }
#else
        public void ToggleKeyboard()
        {

        }
#endif
    }
}