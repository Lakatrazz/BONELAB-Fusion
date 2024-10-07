using UnityEngine;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class BoolElement : MenuElement
    {
#if MELONLOADER
        public BoolElement(IntPtr intPtr) : base(intPtr) { }

        private bool _value = false;
        public bool Value
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

        public event Action<bool> OnValueChanged;

        private GameObject _falseObject = null;
        private GameObject _trueObject = null;

        protected override void Awake()
        {
            base.Awake();

            var falseTransform = transform.Find("False Object");

            if (falseTransform != null)
            {
                _falseObject = falseTransform.gameObject;
            }

            var trueTransform = transform.Find("True Object");

            if (trueTransform != null)
            {
                _trueObject = trueTransform.gameObject;
            }
        }

        public void Toggle() 
        {
            Value = !Value;
        }

        public override void UpdateText()
        {
            if (Text != null)
            {
                Text.text = Title;
                Text.color = Color;
            }

            if (_trueObject != null)
            {
                _trueObject.SetActive(Value);
            }

            if (_falseObject != null)
            {
                _falseObject.SetActive(!Value);
            }
        }
#else
        public void Toggle()
        {

        }
#endif
    }
}