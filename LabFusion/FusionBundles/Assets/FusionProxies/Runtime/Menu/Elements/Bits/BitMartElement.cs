#if MELONLOADER
using LabFusion.Downloading.ModIO;
using LabFusion.Menu;
using LabFusion.UI.Popups;

using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class BitMartElement : MenuElement
    {
#if MELONLOADER
        public BitMartElement(IntPtr intPtr) : base(intPtr) { }

        public MenuPage BitMartPage { get; set; } = null;

        public BitWelcomeElement WelcomeElement { get; set; } = null;

        public BitCatalogElement CatalogElement { get; set; } = null;

        public MenuPage BitDownloadPage { get; set; } = null;
        public FunctionElement BitDownloadYesElement { get; set; } = null;
        public FunctionElement BitDownloadNoElement { get; set; } = null;

        private bool _hasElements = false;

        private void Awake()
        {
            GetElements();
        }

        public void GetElements()
        {
            if (_hasElements)
            {
                return;
            }

            BitMartPage = GetComponent<MenuPage>();

            WelcomeElement = transform.Find("panel_BitWelcome").GetComponent<BitWelcomeElement>();
            WelcomeElement.GetElements();

            WelcomeElement.ViewShopElement.Do(() =>
            {
                BitMartPage.SelectSubPage(CatalogElement.CatalogPage);
            });

            WelcomeElement.DownloadCosmeticsElement.Do(() =>
            {
                BitMartPage.SelectSubPage(BitDownloadPage);
            });

            CatalogElement = transform.Find("panel_BitCatalog").GetComponent<BitCatalogElement>();
            CatalogElement.GetElements();

            BitDownloadPage = transform.Find("panel_BitDownload").GetComponent<MenuPage>();
            BitDownloadYesElement = BitDownloadPage.transform.Find("layout_Options/button_Yes").GetComponent<FunctionElement>()
                .Do(() =>
                {
                    DownloadOfficialCosmetics();
                    BitMartPage.SelectSubPage(WelcomeElement.WelcomePage);
                });

            BitDownloadNoElement = BitDownloadPage.transform.Find("layout_Options/button_No").GetComponent<FunctionElement>()
                .Do(() =>
                {
                    BitMartPage.SelectSubPage(WelcomeElement.WelcomePage);
                });

            _hasElements = true;
        }

        private static void DownloadOfficialCosmetics()
        {
            ModIODownloader.EnqueueDownload(new ModTransaction()
            {
                ModFile = new ModIOFile(ModReferences.FusionCosmeticsId),
                Callback = (info) =>
                {
                    if (info.result != Downloading.ModResult.SUCCEEDED)
                    {
                        Notifier.Send(new Notification()
                        {
                            Title = "Download Failed",
                            Message = "Failed to download Fusion Cosmetics!",
                            SaveToMenu = false,
                            ShowPopup = true,
                            Type = NotificationType.ERROR,
                        });
                    }
                },
            });
        }
#endif
    }
}