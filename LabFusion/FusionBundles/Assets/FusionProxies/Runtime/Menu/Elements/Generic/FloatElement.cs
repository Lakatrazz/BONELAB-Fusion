using UnityEngine;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class FloatElement : ValueElement
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

        public override object GetValue()
        {
            return Mathf.Round(Value * 1000f) / 1000f;
        }

        protected override void OnClearValues()
        {
            _value = 0f;

            MinValue = 0f;
            MaxValue = 1f;
            Increment = 0.01f;

            OnValueChanged = null;

            base.OnClearValues();
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