using UnityEngine;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class FloatButton : MenuButton
    {
#if MELONLOADER
        public FloatButton(IntPtr intPtr) : base(intPtr) { }

        private float _value = 0f;
        public float Value
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

        public float MinValue { get; set; } = 0f;

        public float MaxValue { get; set; } = 1f;

        public float Increment { get; set; } = 0.01f;

        public event Action<float> OnValueChanged;

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
                float rounded = Mathf.Round(Value * 1000f) / 1000f;
                Text.text = $"{Title}: {rounded}";
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