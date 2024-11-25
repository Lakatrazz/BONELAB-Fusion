#if MELONLOADER
using MelonLoader;

using UnityEngine.UI;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class FunctionElement : ButtonElement
    {
#if MELONLOADER
        public Action OnPressed;

        public FunctionElement(IntPtr intPtr) : base(intPtr) { }

        private Button _button = null;

        protected override void Awake()
        {
            base.Awake();

            _button = GetComponent<Button>();
        }

        public void Press()
        {
            OnPressed?.Invoke();
        }

        protected override void OnDraw()
        {
            base.OnDraw();

            if (_button != null)
            {
                _button.interactable = Interactable;
            }
        }

        protected override void OnClearValues()
        {
            OnPressed = null;

            base.OnClearValues();
        }
#else
        public void Press()
        {

        }
#endif
    }
}