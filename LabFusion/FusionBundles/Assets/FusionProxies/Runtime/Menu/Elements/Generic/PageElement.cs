#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class PageElement : GroupElement
    {
#if MELONLOADER
        public PageElement(IntPtr intPtr) : base(intPtr) { }

        private List<PageElement> _pages = new();
        public List<PageElement> Pages => _pages;

        private PageElement _root = null;
        public PageElement Root { get { return _root; } }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (Pages.Count > 0)
            {
                SelectPage(Pages[0]);
            }
        }

        protected override void OnElementRemoved(MenuElement element)
        {
            var page = element.TryCast<PageElement>();

            if (page != null && Pages.Contains(page))
            {
                _pages.Remove(page);
            }

            base.OnElementRemoved(element);
        }

        public void Toggle(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void TogglePages(bool visible)
        {
            foreach (var page in Pages)
            {
                page.Toggle(visible);
            }
        }

        public void Select()
        {
            // If we have a root, make the root open this page
            // If not, then this is the root and we can't do anything
            if (Root != null)
            {
                Root.SelectPage(this);
            }
        }

        public void SelectPage(PageElement page)
        {
            // Check if this isn't the root page
            if (Root != null)
            {
                Root.SelectPage(page);
                return;
            }

            // If this page isn't in our list, don't bother
            if (!Pages.Contains(page))
            {
                return;
            }

            TogglePages(false);

            page.Toggle(true);
        }

        public PageElement AddPage(string title)
        {
            if (Root != null)
            {
                return Root.AddPage(title);
            }

            bool noPages = Pages.Count <= 0;

            var page = AddElement<PageElement>(title);
            page._root = this;

            page.gameObject.SetActive(false);

            _pages.Add(page);

            // First page added? Select it
            if (noPages)
            {
                SelectPage(page);
            }

            return page;
        }

        public PageElement AddPage()
        {
            return AddPage("Page");
        }
#endif
    }
}