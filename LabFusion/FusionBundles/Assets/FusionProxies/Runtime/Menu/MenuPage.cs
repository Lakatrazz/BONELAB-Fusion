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
    public class MenuPage : MonoBehaviour
    {
#if MELONLOADER
        public MenuPage(IntPtr intPtr) : base(intPtr) { }

        public static MenuPage SelectedPage { get; private set; } = null;

        public Action OnShown;

        public MenuPage CurrentPage { get; set; } = null;

        private List<MenuPage> _subPages = null;
        public List<MenuPage> SubPages => _subPages;

        public int DefaultPageIndex { get; set; } = 0;

        public MenuPage Parent { get; set; } = null;

        private void Awake()
        {
            GetSubPages();

            if (Parent == null && transform.parent != null)
            {
                Parent = transform.parent.GetComponentInParent<MenuPage>();
            }
        }

        private void GetSubPages()
        {
            _subPages = new List<MenuPage>();

            foreach (var child in transform)
            {
                var page = child.TryCast<Transform>().GetComponent<MenuPage>();

                if (page == null)
                {
                    continue;
                }

                _subPages.Add(page);

                page.Parent = this;
            }
        }

        private void ResetPages()
        {
            HideSubPages();

            SelectSubPage(DefaultPageIndex);
        }

        private void OnEnable()
        {
            ResetPages();
        }

        public void ShowPage()
        {
            gameObject.SetActive(true);

            OnShown?.Invoke();
        }

        public void HidePage()
        {
            gameObject.SetActive(false);
        }

        public void HideSubPages()
        {
            foreach (var page in _subPages)
            {
                page.HidePage();
            }
        }

        // This must be hidden to prevent crashes
        [HideFromIl2Cpp]
        public void SelectSubPage(MenuPage page)
        {
            // Hide the last page
            if (CurrentPage != null)
            {
                CurrentPage.HidePage();
                CurrentPage = null;
            }

            // Show the new page
            CurrentPage = page;
            CurrentPage.ShowPage();

            SelectedPage = page;
        }

        public void SelectSubPage(int index)
        {
            // Make sure we have the pages referenced
            if (_subPages == null)
            {
                return;
            }

            // Select a new page
            if (_subPages.Count > 0 && _subPages.Count > index)
            {
                SelectSubPage(_subPages[index]);
            }
        }
#else
        public void ShowPage()
        {

        }

        public void HidePage()
        {

        }

        public void SelectSubPage(int index)
        {

        }
#endif
    }
}