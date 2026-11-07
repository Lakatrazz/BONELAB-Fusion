#if MELONLOADER
using LabFusion.Menu;

using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class BitWelcomeElement : MenuElement
    {
#if MELONLOADER
        public BitWelcomeElement(IntPtr intPtr) : base(intPtr) { }

        public MenuPage WelcomePage { get; set; } = null;

        public FunctionElement ViewShopElement { get; set; } = null;
        public FunctionElement DownloadCosmeticsElement { get; set; } = null;

        private bool _hasElements = false;

        public void Awake()
        {
            GetElements();
        }
        
        public void GetElements()
        {
            if (_hasElements) 
            { 
                return; 
            }

            WelcomePage = GetComponent<MenuPage>();

            var optionsGrid = transform.Find("grid_Options");
            ViewShopElement = optionsGrid.Find("button_ViewShop").GetComponent<FunctionElement>().WithTitle("View Shop");
            DownloadCosmeticsElement = optionsGrid.Find("button_DownloadCosmetics").GetComponent<FunctionElement>().WithTitle("Download Official Cosmetics");

            _hasElements = true;
        }
#endif
    }
}