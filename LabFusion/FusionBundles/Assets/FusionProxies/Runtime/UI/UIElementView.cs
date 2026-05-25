using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppInterop.Runtime.Attributes;

using LabFusion.Extensions;
using LabFusion.UI.Elements;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class UIElementView : MonoBehaviour
    {
#if MELONLOADER
        public UIElementView(IntPtr intPtr) : base(intPtr) { }

        [HideFromIl2Cpp]
        public UIElementViewReferences References { get; } = new();

        [HideFromIl2Cpp]
        public List<UIElementView> Children { get; } = new();

        [HideFromIl2Cpp]
        public event Action Clicked;

        [HideFromIl2Cpp]
        public event Action Destroyed;

        private UIDirection _direction = UIDirection.Column;

        [HideFromIl2Cpp]
        public UIDirection Direction
        {
            get => _direction;
            set
            {
                if (_direction == value)
                {
                    return;
                }

                _direction = value;

                ApplyDirection();
            }
        }

        [HideFromIl2Cpp]
        public Transform Container
        {
            get
            {
                return Direction switch
                {
                    UIDirection.Row or 
                    UIDirection.RowReverse => References.RowContainer.transform,
                    _ => References.ColumnContainer.transform,
                };
            }
        }

        [HideFromIl2Cpp]
        public UIElement Element { get; private set; } = null;

        public void OnButtonClicked()
        {
            Clicked.InvokeSafe("executing Clicked event");
        }

        [HideFromIl2Cpp]
        public void AddChild(UIElementView child)
        {
            Children.Add(child);

            child.transform.SetParent(Container, false);
        }

        [HideFromIl2Cpp]
        public void RemoveChild(UIElementView child)
        {
            Children.Remove(child);

            GameObject.Destroy(child.gameObject);
        }

        [HideFromIl2Cpp]
        public void RemoveChildren()
        {
            foreach (var child in Children)
            {
                if (child == null)
                {
                    continue;
                }

                GameObject.Destroy(child.gameObject);
            }

            Children.Clear();
        }

        [HideFromIl2Cpp]
        public void AssignElement(UIElement element)
        {
            UnassignElement();

            Element = element;

            RepaintElement(element);

            element.Repainted += OnRepainted;
        }

        public void UnassignElement()
        {
            if (Element == null)
            {
                return;
            }

            Element.Repainted -= OnRepainted;
        }

        [HideFromIl2Cpp]
        public void RepaintElement(UIElement element)
        {
            var style = element.Style;

            var parent = element.Parent;

            if (parent != null)
            {
                var parentStyle = parent.Style;

                var parentIsColumn = parentStyle.Direction == UIDirection.Column || parentStyle.Direction == UIDirection.ColumnReverse;

                var layoutElement = References.LayoutElement;

                layoutElement.preferredWidth = style.Width ?? -1f;
                layoutElement.preferredHeight = style.Height ?? -1f;

                var flexGrow = style.FlexGrow;
                var alignGrow = parentStyle.AlignContent == UIAlign.Stretch ? -1f : 0f;

                layoutElement.flexibleWidth = parentIsColumn ? alignGrow : flexGrow;
                layoutElement.flexibleHeight = parentIsColumn ? flexGrow : alignGrow;
            }

            Direction = style.Direction;

            var margins = style.Margins;
            References.RectLayoutGroup.padding = new(margins.Left, margins.Right, margins.Top, margins.Bottom);

            var padding = style.Padding;
            References.MarginsLayoutGroup.padding = new(padding.Left, padding.Right, padding.Top, padding.Bottom);

            References.BackgroundColorView.color = style.BackgroundColor;

            References.BackgroundImageView.enabled = style.BackgroundImage != null;
            References.BackgroundImageView.texture = style.BackgroundImage;

            var justifyContent = style.JustifyContent;
            var alignContent = style.AlignContent;
            var isColumn = style.Direction == UIDirection.Column || style.Direction == UIDirection.ColumnReverse;
            var isReversed = style.Direction == UIDirection.ColumnReverse || style.Direction == UIDirection.RowReverse;

            int rawAlignment = 0;

            switch (justifyContent)
            {
                case UIJustify.Center:
                    rawAlignment += isColumn ? 3 : 1;
                    break;
                case UIJustify.End:
                    rawAlignment += isColumn ? 6 : 2;
                    break;
            }

            switch (alignContent)
            {
                case UIAlign.Center:
                    rawAlignment += isColumn ? 1 : 3;
                    break;
                case UIAlign.End:
                    rawAlignment += isColumn ? 2 : 6;
                    break;
            }

            if (isReversed)
            {
                int xAlignment = rawAlignment % 3;
                int yAlignment = (rawAlignment - xAlignment) / 3;

                int flippedYAlignment = 2 - yAlignment;

                rawAlignment = flippedYAlignment * 3 + xAlignment;
            }

            rawAlignment %= (int)TextAnchor.LowerRight + 1;

            var childAlignment = (TextAnchor)rawAlignment;

            References.MarginsLayoutGroup.childAlignment = childAlignment;
            References.ColumnContainer.childAlignment = childAlignment;
            References.RowContainer.childAlignment = childAlignment;

            OnRepaintedElement(element);
        }

        protected virtual void OnRepaintedElement(UIElement element) { }

        protected virtual void OnGetReferences() { }

        private void OnRepainted()
        {
            if (Element == null)
            {
                return;
            }

            RepaintElement(Element);
        }

        private void GetReferences()
        {
            if (References.HasReferences)
            {
                return;
            }

            References.GetReferences(transform);

            OnGetReferences();
        }

        private void ApplyDirection()
        {
            bool reversed = Direction == UIDirection.ColumnReverse || Direction == UIDirection.RowReverse;

            References.ColumnContainer.reverseArrangement = reversed;
            References.RowContainer.reverseArrangement = reversed;

            var container = Container;

            foreach (var child in Children)
            {
                child.transform.SetParent(container, false);
            }
        }

        private void UpdateColliderSize()
        {
            if (!References.HasReferences)
            {
                return;
            }

            var rect = References.RectTransform.rect;
            var collider = References.ClickableColliderView;

            collider.size = new Vector3(rect.width, rect.height, 10f);
        }

        private void Awake()
        {
            GetReferences();
        }

        private void OnDestroy()
        {
            UnassignElement();

            Destroyed?.InvokeSafe("executing Destroyed event");
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateColliderSize();
        }
#else
        public void OnButtonClicked() { }
#endif
    }
}