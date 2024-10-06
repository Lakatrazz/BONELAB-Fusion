using UnityEngine;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class IntButton : MenuButton
    {
#if MELONLOADER
        public IntButton(IntPtr intPtr) : base(intPtr) { }

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
            Value += Increment;

            if (Value > MaxValue)
            {
                Value = MaxValue;
            }

            UpdateSettings();
        }

        public void PreviousValue()
        {
            Value -= Increment;

            if (Value < MinValue)
            {
                Value = MinValue;
            }

            UpdateSettings();
        }

        public override void UpdateText()
        {
            if (Text != null)
            {
                Text.text = $"{Title}: {Value}";
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