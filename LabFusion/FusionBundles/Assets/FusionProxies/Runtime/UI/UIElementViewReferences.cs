#if MELONLOADER
using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Marrow.Integration
{
    public class UIElementViewReferences
    {
        public RectTransform RectTransform { get; private set; } = null;

        public VerticalLayoutGroup RectLayoutGroup { get; private set; } = null;

        public RectTransform MarginsTransform { get; private set; } = null;

        public VerticalLayoutGroup MarginsLayoutGroup { get; private set; } = null;

        public LayoutElement LayoutElement { get; private set; } = null;

        public Image BackgroundColorView { get; private set; } = null;

        public RawImage BackgroundImageView { get; private set; } = null;

        public Button ClickableButtonView { get; private set; } = null;

        public BoxCollider ClickableColliderView { get; private set; } = null;

        public VerticalLayoutGroup ColumnContainer { get; private set; } = null;

        public HorizontalLayoutGroup RowContainer { get; private set; } = null;

        public bool HasReferences { get; private set; } = false;

        public void GetReferences(Transform transform)
        {
            if (HasReferences)
            {
                return;
            }

            RectTransform = transform.GetComponent<RectTransform>();
            RectLayoutGroup = RectTransform.GetComponent<VerticalLayoutGroup>();

            MarginsTransform = transform.Find("view_Margins").GetComponent<RectTransform>();
            MarginsLayoutGroup = MarginsTransform.GetComponent<VerticalLayoutGroup>();

            LayoutElement = transform.GetComponent<LayoutElement>();

            BackgroundColorView = MarginsTransform.Find("view_BackgroundColor").GetComponent<Image>();

            BackgroundImageView = MarginsTransform.Find("view_BackgroundImage").GetComponent<RawImage>();

            var clickableView = MarginsTransform.Find("view_Clickable");
            ClickableButtonView = clickableView.GetComponent<Button>();
            ClickableColliderView = clickableView.GetComponent<BoxCollider>();

            ColumnContainer = MarginsTransform.Find("view_Column").GetComponent<VerticalLayoutGroup>();
            RowContainer = MarginsTransform.Find("view_Row").GetComponent<HorizontalLayoutGroup>();

            HasReferences = true;
        }
    }
}
#endif