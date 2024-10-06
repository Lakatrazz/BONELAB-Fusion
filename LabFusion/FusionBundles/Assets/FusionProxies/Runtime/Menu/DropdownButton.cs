#if MELONLOADER
using Il2CppTMPro;

using MelonLoader;

using UnityEngine;

#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class DropdownButton : MenuGroup
    {
#if MELONLOADER
        public DropdownButton(IntPtr intPtr) : base(intPtr) { }
        
        private bool _expanded = false;
        public bool Expanded => _expanded;

        private Transform _arrowTransform = null;

        protected override void Awake()
        {
            base.Awake();

            var textTransform = transform.Find("Background/text_Label");

            if (textTransform != null)
            {
                Text = textTransform.GetComponent<TMP_Text>();
            }

            _arrowTransform = transform.Find("Background/image_Arrow");
        }

        protected override void OnEnable()
        {
            Collapse();

            base.OnEnable();
        }

        public void Toggle()
        {
            if (Expanded)
            {
                Collapse();
            }
            else
            {
                Expand();
            }
        }

        protected override void OnElementAdded(MenuButton element)
        {
            element.gameObject.SetActive(Expanded);
        }

        public void Expand()
        {
            foreach (var button in Buttons)
            {
                button.gameObject.SetActive(true);
            }

            _expanded = true;

            if (_arrowTransform != null)
            {
                _arrowTransform.localRotation = Quaternion.identity;
            }
        }

        public void Collapse()
        {
            foreach (var button in Buttons)
            {
                button.gameObject.SetActive(false);
            }

            _expanded = false;

            if (_arrowTransform != null)
            {
                _arrowTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }
#else
        public void Toggle()
        {
        }

        public void Expand()
        {
        }

        public void Collapse()
        {
        }
#endif
    }
}