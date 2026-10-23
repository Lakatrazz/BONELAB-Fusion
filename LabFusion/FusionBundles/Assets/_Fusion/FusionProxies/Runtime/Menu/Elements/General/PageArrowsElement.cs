#if MELONLOADER
using Il2CppInterop.Runtime.Attributes;

using LabFusion.Math;
using LabFusion.Menu;

using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class PageArrowsElement : MenuElement
    {
#if MELONLOADER
        public PageArrowsElement(IntPtr intPtr) : base(intPtr) { }

        public FunctionElement BackArrowElement { get; set; } = null;
        public FunctionElement NextArrowElement { get; set; } = null;
        public LabelElement PageCountLabel { get; set; } = null;

        private int _pageIndex = 0;
        public int PageIndex
        {
            get
            {
                return _pageIndex;
            }
            set
            {
                _pageIndex = ManagedMathf.Clamp(value, 0, PageCount - 1);

                Draw();

                OnPageIndexChanged?.Invoke(value);
            }
        }

        private int _pageCount = 0;
        public int PageCount
        {
            get
            {
                if (_pageCount <= 0)
                {
                    return 1;
                }

                return _pageCount;
            }
            set
            {
                _pageCount = value;

                _pageIndex = ManagedMathf.Clamp(_pageIndex, 0, PageCount - 1);

                Draw();
            }
        }

        [HideFromIl2Cpp]
        public event Action<int> OnPageIndexChanged;

        private bool _hasReferences = false;

        private void Awake()
        {
            GetReferences();
        }

        public void GetReferences()
        {
            if (_hasReferences)
            {
                return;
            }

            BackArrowElement = transform.Find("button_BackArrow").GetComponent<FunctionElement>().Do(PreviousPage);
            NextArrowElement = transform.Find("button_NextArrow").GetComponent<FunctionElement>().Do(NextPage);
            PageCountLabel = transform.Find("label_PageCount").GetComponent<LabelElement>();

            _hasReferences = true;
        }

        public void PreviousPage()
        {
            if (PageIndex > 0)
            {
                PageIndex--;
            }
        }

        public void NextPage()
        {
            if (PageIndex + 1 < PageCount) 
            {
                PageIndex++;
            }
        }

        protected override void OnDraw()
        {
            PageCountLabel.Title = $"Page {PageIndex + 1}/{PageCount}";

            BackArrowElement.gameObject.SetActive(PageIndex > 0);
            NextArrowElement.gameObject.SetActive(PageIndex + 1 < PageCount);
        }
#endif
    }
}