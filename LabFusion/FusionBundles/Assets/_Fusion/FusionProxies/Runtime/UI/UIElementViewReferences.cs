#if MELONLOADER
using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Marrow.Integration
{
    public class UIElementViewReferences
    {
        public RectTransform RectTransform { get; private set; } = null;

        public VerticalLayoutGroup RectLayoutGroup { get; private set; } = null;

        public LayoutElement RectLayoutElement { get; private set; } = null;

        public RectTransform MarginsTransform { get; private set; } = null;

        public VerticalLayoutGroup MarginsLayoutGroup { get; private set; } = null;

        public LayoutElement MarginsLayoutElement { get; private set; } = null;

        public Image BackgroundColorView { get; private set; } = null;

        public RawImage BackgroundImageView { get; private set; } = null;

        public Button ClickableButtonView { get; private set; } = null;

        public BoxCollider ClickableColliderView { get; private set; } = null;

        public VerticalLayoutGroup ColumnContainer { get; private set; } = null;

        public HorizontalLayoutGroup RowContainer { get; private set; } = null;

        public VerticalLayoutGroup SubColumnTemplate { get; private set; } = null;

        public HorizontalLayoutGroup SubRowTemplate { get; private set; } = null;

        public bool HasReferences { get; private set; } = false;

        public List<VerticalLayoutGroup> SubColumns { get; } = new();
        public List<HorizontalLayoutGroup> SubRows { get; } = new();

        private bool _hasActiveSubColumns = false;
        private bool _hasActiveSubRows = false;

        public void ClearSubLayouts()
        {
            if (_hasActiveSubColumns)
            {
                foreach (var column in SubColumns)
                {
                    column.gameObject.SetActive(false);
                }

                _hasActiveSubColumns = false;
            }

            if (_hasActiveSubRows)
            {
                foreach (var row in SubRows)
                {
                    row.gameObject.SetActive(false);
                }

                _hasActiveSubRows = false;
            }
        }

        public VerticalLayoutGroup GetSubColumn(int index)
        {
            _hasActiveSubColumns = true;

            if (index < SubColumns.Count)
            {
                var existingColumn = SubColumns[index];
                existingColumn.gameObject.SetActive(true);
                return existingColumn;
            }

            var newColumn = GameObject.Instantiate(SubColumnTemplate, SubColumnTemplate.transform.parent);
            newColumn.gameObject.name = $"view_SubColumn [{index}]";
            newColumn.gameObject.SetActive(true);
            SubColumns.Add(newColumn);

            return newColumn;
        }

        public HorizontalLayoutGroup GetSubRow(int index)
        {
            _hasActiveSubRows = true;

            if (index < SubRows.Count)
            {
                var existingRow = SubRows[index];
                existingRow.gameObject.SetActive(true);
                return existingRow;
            }

            var newRow = GameObject.Instantiate(SubRowTemplate, SubRowTemplate.transform.parent);
            newRow.gameObject.name = $"view_SubRow [{index}]";
            newRow.gameObject.SetActive(true);
            SubRows.Add(newRow);

            return newRow;
        }

        public void GetReferences(Transform transform)
        {
            if (HasReferences)
            {
                return;
            }

            RectTransform = transform.GetComponent<RectTransform>();
            RectLayoutGroup = RectTransform.GetComponent<VerticalLayoutGroup>();
            RectLayoutElement = transform.GetComponent<LayoutElement>();

            MarginsTransform = transform.Find("view_Margins").GetComponent<RectTransform>();
            MarginsLayoutGroup = MarginsTransform.GetComponent<VerticalLayoutGroup>();
            MarginsLayoutElement = MarginsTransform.GetComponent<LayoutElement>();

            BackgroundColorView = MarginsTransform.Find("view_BackgroundColor").GetComponent<Image>();

            BackgroundImageView = MarginsTransform.Find("view_BackgroundImage").GetComponent<RawImage>();

            var clickableView = MarginsTransform.Find("view_Clickable");
            ClickableButtonView = clickableView.GetComponent<Button>();
            ClickableColliderView = clickableView.GetComponent<BoxCollider>();

            ColumnContainer = MarginsTransform.Find("view_Column").GetComponent<VerticalLayoutGroup>();
            RowContainer = MarginsTransform.Find("view_Row").GetComponent<HorizontalLayoutGroup>();

            CreateTemplates();

            HasReferences = true;
        }

        private void CreateTemplates()
        {
            SubColumnTemplate = GameObject.Instantiate(ColumnContainer, null, false);
            SubColumnTemplate.gameObject.SetActive(false);
            SubColumnTemplate.gameObject.name = "view_SubColumn [Template]";

            SubRowTemplate = GameObject.Instantiate(RowContainer, null, false);
            SubRowTemplate.gameObject.SetActive(false);
            SubRowTemplate.gameObject.name = "view_SubRow [Template]";

            SubColumnTemplate.transform.SetParent(RowContainer.transform, false);
            SubRowTemplate.transform.SetParent(ColumnContainer.transform, false);
        }
    }
}
#endif