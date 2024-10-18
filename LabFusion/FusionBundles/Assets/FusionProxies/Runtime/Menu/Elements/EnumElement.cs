#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class EnumElement : LabelElement
    {
#if MELONLOADER
        public EnumElement(IntPtr intPtr) : base(intPtr) { }

        private Enum _value = null;
        public Enum Value
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

        private Type _enumType = null;
        public Type EnumType
        {
            get
            {
                return _enumType;
            }
            set
            {
                _enumType = value;

                if (value != null)
                {
                    _enumValues = Enum.GetValues(_enumType);

                    // Apply default enum if its not set
                    _value ??= _enumValues.GetValue(0) as Enum;
                }
            }
        }

        public Action<Enum> OnValueChanged;

        private int _enumIndex = 1;
        private Array _enumValues = null;

        public void NextValue() 
        {
            if (_enumValues == null)
            {
                return;
            }

            _enumIndex %= _enumValues.Length;
            Value = _enumValues.GetValue(_enumIndex++) as Enum;
        }

        public void PreviousValue()
        {
            if (_enumValues == null)
            {
                return;
            }

            _enumIndex %= _enumValues.Length;
            Value = _enumValues.GetValue(_enumIndex--) as Enum;
        }

        public override void UpdateText()
        {
            if (Text != null)
            {
                Text.text = $"{Title}: {Value}";
                Text.color = Color;
            }
        }
#else
        public void NextValue()
        {

        }

        public void PreviousValue()
        {

        }
#endif
    }
}