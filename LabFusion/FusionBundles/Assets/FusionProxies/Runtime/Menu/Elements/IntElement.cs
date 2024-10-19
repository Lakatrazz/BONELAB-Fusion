using UnityEngine.UI;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class IntElement : ValueElement
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

                Draw();

                OnValueChanged?.Invoke(value);
            }
        }

        public int MinValue { get; set; } = 0;

        public int MaxValue { get; set; } = 1;

        public int Increment { get; set; } = 1;

        public Action<int> OnValueChanged;

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
            return Value;
        }

        protected override void OnClearValues()
        {
            _value = 0;

            MinValue = 0;
            MaxValue = 1;
            Increment = 1;

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