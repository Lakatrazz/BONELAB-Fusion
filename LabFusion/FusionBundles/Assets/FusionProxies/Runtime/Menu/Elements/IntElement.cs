using UnityEngine.UI;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class IntElement : ButtonElement
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

        private Button _leftArrow = null;
        private Button _rightArrow = null;

        protected override void Awake()
        {
            base.Awake();

            _leftArrow = transform.Find("button_LeftArrow").GetComponent<Button>();
            _rightArrow = transform.Find("button_RightArrow").GetComponent<Button>();
        }

        protected override void OnDraw()
        {
            base.OnDraw();

            if (_leftArrow != null)
            {
                _leftArrow.gameObject.SetActive(Interactable);
            }

            if (_rightArrow != null)
            {
                _rightArrow.gameObject.SetActive(Interactable);
            }
        }

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