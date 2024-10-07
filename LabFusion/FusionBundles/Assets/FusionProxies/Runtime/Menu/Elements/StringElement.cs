#if MELONLOADER
using LabFusion.Menu;

using MelonLoader;

using UnityEngine;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class StringElement : MenuElement
    {
#if MELONLOADER
        public StringElement(IntPtr intPtr) : base(intPtr) { }

        public event Action<string> OnValueChanged;

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

                UpdateSettings();

                OnValueChanged?.Invoke(value);
            }
        }

        public void ToggleKeyboard()
        {
            if (MenuKeyboardHelper.KeyboardOpened)
            {
                MenuKeyboardHelper.CloseKeyboard();
            }
            else
            {
                MenuKeyboardHelper.AssignKeyboardToButton(this);
            }
        }

        public override void UpdateText()
        {
            if (Text != null)
            {
                if (string.IsNullOrEmpty(Value))
                {
                    Text.text = $"Click to add {Title.ToLower()}...";
                    Text.color = Color.gray * Color;
                }
                else
                {
                    Text.text = $"{Title}: {Value}";
                    Text.color = Color;
                }
            }
        }
#else
        public void ToggleKeyboard()
        {

        }
#endif
    }
}