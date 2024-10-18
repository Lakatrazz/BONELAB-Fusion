using UnityEngine;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class FloatElement : LabelElement
    {
#if MELONLOADER
        public FloatElement(IntPtr intPtr) : base(intPtr) { }

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

                Draw();

                OnValueChanged?.Invoke(value);
            }
        }

        public float MinValue { get; set; } = 0f;

        public float MaxValue { get; set; } = 1f;

        public float Increment { get; set; } = 0.01f;

        public Action<float> OnValueChanged;

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
                float rounded = Mathf.Round(Value * 1000f) / 1000f;
                Text.text = $"{Title}: {rounded}";

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