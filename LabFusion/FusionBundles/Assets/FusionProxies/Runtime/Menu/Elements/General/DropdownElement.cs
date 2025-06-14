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
    public class DropdownElement : GroupElement
    {
#if MELONLOADER
        public DropdownElement(IntPtr intPtr) : base(intPtr) { }
        
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

        protected override void OnElementAdded(MenuElement element)
        {
            element.gameObject.SetActive(Expanded);
        }

        public void Expand()
        {
            foreach (var element in Elements)
            {
                element.gameObject.SetActive(true);
            }

            _expanded = true;

            if (_arrowTransform != null)
            {
                _arrowTransform.localRotation = Quaternion.identity;
            }
        }

        public void Collapse()
        {
            foreach (var element in Elements)
            {
                element.gameObject.SetActive(false);
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