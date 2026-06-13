using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppInterop.Runtime.Attributes;

using LabFusion.Extensions;
using LabFusion.UI.Elements;
using LabFusion.UI.Styles;
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
        public UIElementView Parent { get; private set; } = null;

        [HideFromIl2Cpp]
        public int SiblingIndex { get; private set; } = -1;

        [HideFromIl2Cpp]
        public List<UIElementView> Children { get; } = new();

        [HideFromIl2Cpp]
        public event Action Clicked;

        [HideFromIl2Cpp]
        public event Action Destroyed;

        private Direction _direction = Direction.Column;

        [HideFromIl2Cpp]
        public Direction Direction
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

        private int _flexWrapCount = 0;
        public int FlexWrapCount
        {
            get => _flexWrapCount;
            set
            {
                if (_flexWrapCount == value)
                {
                    return;
                }

                _flexWrapCount = value;

                ApplyWrapping();
            }
        }

        private Position _position = Position.Relative;

        [HideFromIl2Cpp]
        public Position Position
        {
            get => _position;
            set
            {
                if (_position == value)
                {
                    return;
                }

                _position = value;

                if (Parent != null)
                {
                    Parent.ReparentChild(this);
                }
            }
        }

        [HideFromIl2Cpp]
        public Transform Container
        {
            get
            {
                return Direction switch
                {
                    Direction.Row or 
                    Direction.RowReverse => References.RowContainer.transform,
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
            var siblingIndex = Children.Count;

            Children.Add(child);

            child.Parent = Parent;
            child.SiblingIndex = siblingIndex;

            ReparentChild(child);
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

            if (element == null)
            {
                return;
            }

            Element = element;

            Repaint();

            element.ContentGenerated += OnContentGenerated;
            element.ChildrenGenerated += OnChildrenGenerated;
            element.StyleResolved += OnStyleResolved;
        }

        public void UnassignElement()
        {
            if (Element == null)
            {
                return;
            }

            Element.ContentGenerated -= OnContentGenerated;
            Element.ChildrenGenerated -= OnChildrenGenerated;
            Element.StyleResolved -= OnStyleResolved;

            Element = null;
        }

        public void Repaint()
        {
            RepaintStyle();
            RepaintContent();
            RepaintChildren();
        }

        public void RepaintContent()
        {
            OnContentRepainted();
        }

        public void RepaintChildren()
        {
            RemoveChildren();

            var spawner = UIElementSpawner.Instance;

            foreach (var childElement in Element.PhysicalChildren)
            {
                var childElementView = spawner.CreateElementView(childElement, References.MarginsTransform);

                AddChild(childElementView);
            }

            OnChildrenRepainted();
        }

        public void RepaintStyle()
        {
            var style = Element.ResolvedStyle;

            Direction = style.Direction;
            FlexWrapCount = style.FlexWrapCount.GetValueOrDefault(StyleDefaults.FlexWrapCount);
            Position = style.Position;

            var parent = Element.Parent;

            if (parent != null)
            {
                var parentStyle = parent.ResolvedStyle;

                var parentDirection = parentStyle.Direction.GetValueOrDefault(StyleDefaults.Direction);

                var parentIsColumn = parentDirection == Direction.Column || parentDirection == Direction.ColumnReverse;

                var marginsLayoutElement = References.MarginsLayoutElement;

                marginsLayoutElement.preferredWidth = style.Width.GetValueOrDefault(StyleDefaults.Width);
                marginsLayoutElement.preferredHeight = style.Height.GetValueOrDefault(StyleDefaults.Height);

                var rectLayoutElement = References.RectLayoutElement;

                bool alignStretch;

                if (style.AlignSelfStretch.HasValue())
                {
                    alignStretch = style.AlignSelfStretch.Value;
                }
                else
                {
                    var parentAlignItems = parentStyle.AlignItems.GetValueOrDefault(StyleDefaults.AlignItems);
                    alignStretch = parentAlignItems == Align.Stretch;
                }

                var flexGrow = style.FlexGrow.GetValueOrDefault(StyleDefaults.FlexGrow);
                var alignGrow = alignStretch ? -1f : 0f;

                rectLayoutElement.flexibleWidth = parentIsColumn ? alignGrow : flexGrow;
                rectLayoutElement.flexibleHeight = parentIsColumn ? flexGrow : alignGrow;

                bool ignoreLayout = style.Position.GetValueOrDefault(StyleDefaults.Position) == Position.Absolute;

                rectLayoutElement.ignoreLayout = ignoreLayout;

                if (ignoreLayout)
                {
                    var rectTransform = References.RectTransform;

                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;

                    var absoluteOffset = style.AbsoluteOffset;

                    rectTransform.offsetMin = absoluteOffset;
                    rectTransform.offsetMax = absoluteOffset;
                }
            }

            BorderOffsets margins = style.Margins;
            References.RectLayoutGroup.padding = new(margins.Left, margins.Right, margins.Top, margins.Bottom);

            BorderOffsets padding = style.Padding;
            References.MarginsLayoutGroup.padding = new(padding.Left, padding.Right, padding.Top, padding.Bottom);

            References.BackgroundColorView.color = style.BackgroundColor.GetValueOrDefault(StyleDefaults.BackgroundColor);

            References.BackgroundImageView.enabled = style.BackgroundImage.HasValue();
            References.BackgroundImageView.texture = style.BackgroundImage;

            var direction = style.Direction.GetValueOrDefault(StyleDefaults.Direction);

            Justify justifyContent = style.JustifyContent.GetValueOrDefault(StyleDefaults.JustifyContent);
            Align alignItems = style.AlignItems.GetValueOrDefault(StyleDefaults.AlignItems);
            var isColumn = direction == Direction.Column || direction == Direction.ColumnReverse;
            var isReversed = direction == Direction.ColumnReverse || direction == Direction.RowReverse;

            int rawAlignment = 0;

            switch (justifyContent)
            {
                case Justify.Center:
                    rawAlignment += isColumn ? 3 : 1;
                    break;
                case Justify.End:
                    rawAlignment += isColumn ? 6 : 2;
                    break;
            }

            switch (alignItems)
            {
                case Align.Center:
                    rawAlignment += isColumn ? 1 : 3;
                    break;
                case Align.End:
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

            OnStyleRepainted();
        }

        protected virtual void OnContentRepainted() { }
        protected virtual void OnChildrenRepainted() { }
        protected virtual void OnStyleRepainted() { }

        protected virtual void OnGetReferences() { }

        private void OnContentGenerated() => RepaintContent();

        private void OnChildrenGenerated() => RepaintChildren();

        private void OnStyleResolved() => RepaintStyle();

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
            bool reversed = Direction == Direction.ColumnReverse || Direction == Direction.RowReverse;

            References.ColumnContainer.reverseArrangement = reversed;
            References.RowContainer.reverseArrangement = reversed;

            ReparentChildren();
        }

        private void ApplyWrapping()
        {
            ReparentChildren();
        }

        private void ReparentChildren()
        {
            foreach (var child in Children)
            {
                ReparentChild(child);
            }
        }

        private void ReparentChild(UIElementView child)
        {
            var parent = GetContainerForChild(child);

            child.transform.SetParent(parent, false);
        }

        private Transform GetContainerForChild(UIElementView child)
        {
            var position = child.Position;

            if (position == Position.Absolute)
            {
                return References.MarginsTransform;
            }

            if (FlexWrapCount <= 0)
            {
                return Direction switch
                {
                    Direction.Row or
                    Direction.RowReverse => References.RowContainer.transform,
                    _ => References.ColumnContainer.transform,
                };
            }

            var siblingIndex = child.SiblingIndex;
            var groupIndex = Mathf.FloorToInt((float)siblingIndex / (float)FlexWrapCount);

            return Direction switch
            {
                Direction.Row or 
                Direction.RowReverse => References.GetSubRow(groupIndex).transform,
                _ => References.GetSubColumn(groupIndex).transform,
            };
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

        private void UpdateElement()
        {
            if (Element == null)
            {
                return;
            }

            if (Element.IsStyleDirty)
            {
                Element.ResolveStyle();
            }

            if (Element.IsContentDirty)
            {
                Element.GenerateContent();
            }

            if (Element.IsChildrenDirty)
            {
                Element.GenerateChildren();
            }
        }

        private void Awake()
        {
            GetReferences();
        }

        private void Update()
        {
            UpdateElement();
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