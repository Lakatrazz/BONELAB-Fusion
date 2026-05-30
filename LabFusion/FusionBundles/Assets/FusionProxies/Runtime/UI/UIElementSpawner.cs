using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.UI.Elements;

using Il2CppInterop.Runtime.Attributes;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class UIElementSpawner : MonoBehaviour
    {
#if MELONLOADER
        public static UIElementSpawner Instance { get; private set; } = null;

        [HideFromIl2Cpp]
        public Transform TemplateContainer { get; private set; } = null;

        [HideFromIl2Cpp]
        public Dictionary<Type, UIElementView> TypeToTemplateElements { get; } = new();

        [HideFromIl2Cpp]
        public UIElementView CreateElementView(UIElement element, Transform parent)
        {
            UIElementView view;

            if (element is TextElement)
            {
                view = CreateElementView<TextElementView>(parent);
            }
            else
            {
                view = CreateElementView<UIElementView>(parent);
            }

            view.AssignElement(element);
            return view;
        }

        [HideFromIl2Cpp]
        public TElementView CreateElementView<TElementView>(Transform parent) where TElementView : UIElementView
        {
            if (TypeToTemplateElements.TryGetValue(typeof(TElementView), out var template))
            {
                var instance = GameObject.Instantiate(template, parent, false);

                instance.name = template.name;

                return instance.TryCast<TElementView>();
            }

            return null;
        }

        private void GetTemplates()
        {
            TypeToTemplateElements.Clear();

            TemplateContainer = transform.Find("Template Container");

            for (var i = 0; i < TemplateContainer.childCount; i++)
            {
                var child = TemplateContainer.GetChild(i);

                var template = child.GetComponent<UIElementView>();

                if (template == null)
                {
                    continue;
                }

                var type = template.GetType();

                TypeToTemplateElements[type] = template;
            }
        }

        private void Awake()
        {
            Instance = this;

            GetTemplates();
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }
#endif
    }
}