#if MELONLOADER
using MelonLoader;

using UnityEngine.UI;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class ValueElement : ButtonElement
    {
#if MELONLOADER
        public ValueElement(IntPtr intPtr) : base(intPtr) { }

        public override string DefaultTextFormat => "{0}: {1}";

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

        public override void UpdateText()
        {
            if (Text != null)
            {
                Text.text = string.Format(TextFormat, Title, GetValue());
                Text.color = Color;
            }
        }

        public virtual object GetValue()
        {
            return null;
        }
#endif
    }
}