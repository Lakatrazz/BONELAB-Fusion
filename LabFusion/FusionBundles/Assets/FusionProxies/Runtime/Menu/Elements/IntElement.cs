using UnityEngine;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class IntElement : MenuElement
    {
#if MELONLOADER
        public IntElement(IntPtr intPtr) : base(intPtr) { }

        private int _value = 0;
        public int Value
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

        public int MinValue { get; set; } = 0;

        public int MaxValue { get; set; } = 1;

        public int Increment { get; set; } = 1;

        public event Action<int> OnValueChanged;

        public void NextValue() 
        {
            var newValue = Value + Increment;

            if (newValue > MaxValue)
            {
                newValue = MaxValue;
            }

            Value = newValue;
        }

        public void PreviousValue()
        {
            var newValue = Value - Increment;

            if (newValue < MinValue)
            {
                newValue = MinValue;
            }

            Value = newValue;
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