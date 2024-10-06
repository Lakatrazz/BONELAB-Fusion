#if MELONLOADER
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

        public MenuPage CurrentPage { get; set; } = null;

        private List<MenuPage> _subPages = null;

        private void Awake()
        {
            GetSubPages();
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
            }
        }

        private void OnEnable()
        {
            HideSubPages();

            SelectSubPage(0);
        }

        public void ShowPage()
        {
            gameObject.SetActive(true);
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

        public void SelectSubPage(int index)
        {
            // Hide the last page
            if (CurrentPage != null)
            {
                CurrentPage.HidePage();
                CurrentPage = null;
            }

            // Select a new page
            if (_subPages.Count > 0 && _subPages.Count > index)
            {
                CurrentPage = _subPages[index];
                CurrentPage.ShowPage();
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