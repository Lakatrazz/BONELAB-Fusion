#if MELONLOADER
using Il2CppInterop.Runtime.Attributes;

using MelonLoader;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class GroupElement : LabelElement
    {
#if MELONLOADER
        public GroupElement(IntPtr intPtr) : base(intPtr) { }

        public List<MenuElement> Elements => _elements;

        public List<MenuElement> Templates => _elementTemplates;

        private readonly List<MenuElement> _elements = new();
        private readonly List<MenuElement> _elementTemplates = new();

        private bool _hasTemplates = false;

        protected override void Awake()
        {
            base.Awake();

            GetTemplates();
        }

        private void GetTemplates()
        {
            if (_hasTemplates)
            {
                return;
            }

            foreach (var child in transform)
            {
                var childTransform = child.TryCast<Transform>();

                var childTemplate = childTransform.GetComponent<MenuElement>();

                if (childTemplate != null)
                {
                    _elementTemplates.Add(childTemplate);
                }
            }

            _hasTemplates = true;
        }

        [HideFromIl2Cpp]
        protected virtual void OnElementAdded(MenuElement element) 
        {
            element.gameObject.SetActive(true);
        }

        [HideFromIl2Cpp]
        protected virtual void OnElementRemoved(MenuElement element)
        {
        }

        [HideFromIl2Cpp]
        public TElement AddElement<TElement>(string title) where TElement : MenuElement
        {
            GetTemplates();

            TElement template = null;

            foreach (var found in _elementTemplates)
            {
                var casted = found.TryCast<TElement>();

                if (casted != null)
                {
                    template = casted;
                    break;
                }
            }

            if (template == null)
            {
                return null;
            }

            var newElement = GameObject.Instantiate(template, transform, false);
            newElement.name = title;
            newElement.Title = title;

            _elements.Add(newElement);

            OnElementAdded(newElement);

            return newElement;
        }

        [HideFromIl2Cpp]
        public TElement AddOrGetElement<TElement>(string title) where TElement : MenuElement
        {
            foreach (var element in Elements)
            {
                if (element.Title != title)
                {
                    continue;
                }

                var casted = element.TryCast<TElement>();

                if (casted != null)
                {
                    return casted;
                }
            }

            return AddElement<TElement>(title);
        }

        [HideFromIl2Cpp]
        public bool RemoveElement<TElement>(TElement element) where TElement : MenuElement
        {
            if (!Elements.Contains(element))
            {
                return false;
            }

            _elements.Remove(element);

            OnElementRemoved(element);

            GameObject.Destroy(element.gameObject);

            return true;
        }

        [HideFromIl2Cpp]
        public int RemoveElements<TElement>() where TElement : MenuElement
        {
            var elementsToRemove = new List<TElement>();
            
            foreach (var element in Elements)
            {
                var casted = element.TryCast<TElement>();

                if (casted == null)
                {
                    continue;
                }

                elementsToRemove.Add(casted);
            }

            int count = 0;

            foreach (var element in elementsToRemove)
            {
                RemoveElement(element);
                count++;
            }

            return count;
        }

        [HideFromIl2Cpp]
        public int RemoveElements() => RemoveElements<MenuElement>();

        protected override void OnClearValues()
        {
            RemoveElements();

            base.OnClearValues();
        }
#endif
    }
}