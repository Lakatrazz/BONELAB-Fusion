using UnityEngine;
using UnityEngine.UI;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class BoolElement : ButtonElement
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

                Draw();

                OnValueChanged?.Invoke(value);
            }
        }

        public Action<bool> OnValueChanged;

        private Button _button = null;
        private GameObject _falseObject = null;
        private GameObject _trueObject = null;

        protected override void Awake()
        {
            base.Awake();

            _button = GetComponent<Button>();

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
            base.UpdateText();

            if (_button != null)
            {
                _button.interactable = Interactable;
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

        protected override void OnClearValues()
        {
            _value = false;
            OnValueChanged = null;

            base.OnClearValues();
        }
#else
        public void Toggle()
        {

        }
#endif
    }
}