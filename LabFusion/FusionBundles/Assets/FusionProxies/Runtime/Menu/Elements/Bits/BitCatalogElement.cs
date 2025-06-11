#if MELONLOADER
using Il2CppInterop.Runtime.Attributes;
using Il2CppTMPro;

using LabFusion.Menu;
using LabFusion.SDK.Points;
using LabFusion.Utilities;

using MelonLoader;

using UnityEngine;
using UnityEngine.UI;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class BitCatalogElement : MenuElement
    {
#if MELONLOADER
        public BitCatalogElement(IntPtr intPtr) : base(intPtr) { }

        public enum CatalogTab
        {
            Shop,
            Inventory,
        }

        public MenuPage CatalogPage { get; set; } = null;

        public MenuPage BrowserPage { get; set; } = null;

        public MenuPage BitPalletsPage { get; set; } = null;
        [HideFromIl2Cpp]
        public FunctionElement[] BitPalletElements { get; set; } = null;

        public MenuPage BitResultsPage { get; set; } = null;
        [HideFromIl2Cpp]
        public BitResultElement[] BitResultElements { get; set; } = null;

        public PageArrowsElement PageArrowsElement { get; set; } = null;

        public FunctionElement ShopElement { get; set; } = null;
        public FunctionElement InventoryElement { get; set; } = null;

        public MenuPage ItemPage { get; set; } = null;

        public RawImage ItemPreview { get; set; } = null;
        public LabelElement ItemAuthorLabel { get; set; } = null;
        public LabelElement ItemVersionLabel { get; set; } = null;
        public LabelElement ItemTagsLabel { get; set; } = null;
        public FunctionElement ItemToggleElement { get; set; } = null;

        public LabelElement ItemNameLabel { get; set; } = null;
        public LabelElement ItemDescriptionLabel { get; set; } = null;
        public LabelElement ItemPriceLabel { get; set; } = null;
        public FunctionElement ItemBuyElement { get; set; } = null;

        public FunctionElement ItemCloseElement { get; set; } = null;
        public LabelElement ItemTitleLabel { get; set; } = null;

        public FunctionElement CatalogCloseElement { get; set; } = null;
        public FunctionElement CatalogSortByElement { get; set; } = null;
        public FunctionElement CatalogUnequipAllElement { get; set; } = null;
        public TMP_Text CatalogBitCountText { get; set; } = null;

        [HideFromIl2Cpp]
        public event Action<PointItem> OnItemPurchased, OnItemEquipped, OnItemUnequipped;

        [HideFromIl2Cpp]
        public event Action OnAllItemsUnequipped;

        private List<PointItem> _pointItems = null;
        private List<string> _pallets = null;
        private Dictionary<string, List<PointItem>> _palletToPointItem = null;

        private string _currentPallet = null;
        private PointItem _currentItem = null;

        private SortMode _sortMode = SortMode.PRICE;

        private CatalogTab _catalogTab = CatalogTab.Shop;

        private bool _hasElements = false;

        private void Awake()
        {
            GetElements();

            PointItemManager.OnBitCountChanged += OnBitCountChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            PointItemManager.OnBitCountChanged -= OnBitCountChanged;
        }

        private void OnBitCountChanged()
        {
            DrawBitCount();
        }

        public void GetElements()
        {
            if (_hasElements) 
            { 
                return; 
            }

            CatalogPage = GetComponent<MenuPage>();

            BrowserPage = transform.Find("panel_Browser").GetComponent<MenuPage>();

            BitPalletsPage = BrowserPage.transform.Find("panel_BitPallets").GetComponent<MenuPage>();
            BitPalletElements = BitPalletsPage.GetComponentsInChildren<FunctionElement>(true);

            PopulateBitPalletElements();

            BitResultsPage = BrowserPage.transform.Find("panel_BitResults").GetComponent<MenuPage>();
            BitResultElements = BitResultsPage.GetComponentsInChildren<BitResultElement>(true);

            PopulateBitResultElements();

            var browserToolbars = BrowserPage.transform.Find("group_Toolbars");

            PageArrowsElement = browserToolbars.Find("button_PageArrows").GetComponent<PageArrowsElement>();
            PageArrowsElement.OnPageIndexChanged += OnPageIndexChanged;

            var browserTabs = browserToolbars.Find("layout_Tabs");
            ShopElement = browserTabs.Find("button_Shop").GetComponent<FunctionElement>().WithTitle("Shop").Do(SelectShopTab);
            InventoryElement = browserTabs.Find("button_Inventory").GetComponent<FunctionElement>().WithTitle("Inventory").Do(SelectInventoryTab);

            ItemPage = transform.Find("panel_BitItem").GetComponent<MenuPage>();

            var informationLayout = ItemPage.transform.Find("layout_Information");

            var previewLayout = informationLayout.Find("layout_Preview");
            ItemPreview = previewLayout.Find("icon_Preview/image_Preview").GetComponent<RawImage>();
            ItemAuthorLabel = previewLayout.Find("label_Author").GetComponent<LabelElement>();
            ItemVersionLabel = previewLayout.Find("label_Version").GetComponent<LabelElement>();
            ItemTagsLabel = previewLayout.Find("label_Tags").GetComponent<LabelElement>();
            ItemToggleElement = previewLayout.Find("button_Toggle").GetComponent<FunctionElement>().Do(ToggleItem);

            var descriptionLayout = informationLayout.Find("layout_Description");
            ItemNameLabel = descriptionLayout.Find("label_Name").GetComponent<LabelElement>();
            ItemDescriptionLabel = descriptionLayout.Find("label_Description").GetComponent<LabelElement>();
            ItemPriceLabel = descriptionLayout.Find("label_Price").GetComponent<LabelElement>();
            ItemBuyElement = descriptionLayout.Find("button_Buy").GetComponent<FunctionElement>().Do(ConfirmPurchase);

            ItemCloseElement = ItemPage.transform.Find("group_Toolbar/button_Close").GetComponent<FunctionElement>().Do(GoToBitResults);
            ItemTitleLabel = ItemPage.transform.Find("group_Toolbar/label_Title").GetComponent<LabelElement>().WithTitle("INFORMATION");

            var optionsLayout = transform.Find("layout_Options");

            CatalogCloseElement = optionsLayout.Find("button_Close").GetComponent<FunctionElement>()
                .Do(() =>
                {
                    CatalogPage.Parent.SelectSubPage(CatalogPage.DefaultPageIndex);
                });

            CatalogSortByElement = optionsLayout.Find("button_SortBy").GetComponent<FunctionElement>().WithTitle($"Sort By: {_sortMode}")
                .Do(NextSortMode);
            CatalogUnequipAllElement = optionsLayout.Find("button_UnequipAll").GetComponent<FunctionElement>().WithTitle("Unequip All")
                .Do(UnequipAll);
            CatalogBitCountText = optionsLayout.Find("label_BitCount/text").GetComponent<TMP_Text>();

            _hasElements = true;
        }

        private void NextSortMode()
        {
            _sortMode++;

            if (_sortMode >= SortMode.LAST_SORT)
            {
                _sortMode = 0;
            }

            CatalogSortByElement.Title = $"Sort By: {_sortMode}";

            Draw();
        }

        private void SelectShopTab()
        {
            _catalogTab = CatalogTab.Shop;

            PageArrowsElement.PageIndex = 0;

            GoToBitPallets();
        }

        private void SelectInventoryTab()
        {
            _catalogTab = CatalogTab.Inventory;

            PageArrowsElement.PageIndex = 0;

            GoToBitPallets();
        }

        private void UnequipAll()
        {
            PointItemManager.UnequipAll();

            OnAllItemsUnequipped?.Invoke();

            LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.UnequipItemReference), transform.position, LocalAudioPlayer.SFXSettings);

            Draw();

            DrawPointItem();
        }

        private void ConfirmPurchase()
        {
            if (!_currentItem.IsUnlocked)
            {
                TryBuyItem();
            }
            else if (!_currentItem.IsMaxUpgrade)
            {
                TryUpgradeItem();
            }
        }

        private void GoToBitPallets()
        {
            CatalogPage.SelectSubPage(BrowserPage);
            BrowserPage.SelectSubPage(BitPalletsPage);

            Draw();
        }

        private void GoToBitResults()
        {
            CatalogPage.SelectSubPage(BrowserPage);
            BrowserPage.SelectSubPage(BitResultsPage);

            Draw();
        }

        private void TryBuyItem()
        {
            if (PointItemManager.TryBuyItem(_currentItem))
            {
                OnItemPurchased?.Invoke(_currentItem);

                GoToBitResults();

                LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.SuccessfulPurchaseReference), transform.position, LocalAudioPlayer.SFXSettings);
            }
            else
            {
                LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.FailedPurchaseReference), transform.position, LocalAudioPlayer.SFXSettings);
            }
        }

        private void TryUpgradeItem()
        {
            if (PointItemManager.TryUpgradeItem(_currentItem))
            {
                GoToBitResults();

                LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.SuccessfulPurchaseReference), transform.position, LocalAudioPlayer.SFXSettings);
            }
            else
            {
                LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.FailedPurchaseReference), transform.position, LocalAudioPlayer.SFXSettings);
            }
        }

        private void ToggleItem()
        {
            if (!_currentItem.IsUnlocked)
            {
                return;
            }

            if (_currentItem.IsEquipped)
            {
                OnItemUnequipped?.Invoke(_currentItem);

                PointItemManager.SetEquipped(_currentItem, false);

                LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.UnequipItemReference), transform.position, LocalAudioPlayer.SFXSettings);
            }
            else
            {
                OnItemEquipped?.Invoke(_currentItem);

                PointItemManager.SetEquipped(_currentItem, true);

                LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.EquipItemReference), transform.position, LocalAudioPlayer.SFXSettings);
            }

            DrawPointItem();
        }

        private void OnPageIndexChanged(int index)
        {
            Draw();
        }

        [HideFromIl2Cpp]
        public void ShowPointItem(PointItem pointItem)
        {
            _currentItem = pointItem;

            CatalogPage.SelectSubPage(ItemPage);

            DrawPointItem();
        }

        private void DrawPointItem()
        {
            var pointItem = _currentItem;

            if (pointItem == null)
            {
                return;
            }

            pointItem.LoadPreviewIcon(texture =>
            {
                if (ItemPreview == null)
                {
                    return;
                }

                ItemPreview.texture = texture;
            });

            ItemAuthorLabel.Title = pointItem.Author;
            ItemVersionLabel.Title = pointItem.Version;

            if (pointItem.Tags != null && pointItem.Tags.Length > 0)
            {
                var tagString = pointItem.Tags[0];

                for (var i = 1; i < pointItem.Tags.Length; i++)
                {
                    tagString += $", {pointItem.Tags[i]}";
                }

                ItemTagsLabel.Title = tagString;
            }
            else
            {
                ItemTagsLabel.Title = "Misc";
            }

            ItemNameLabel.Title = pointItem.Title;
            ItemNameLabel.Color = PointItemManager.ParseColor(pointItem.Rarity);

            ItemDescriptionLabel.Title = pointItem.CurrentDescription;
            ItemPriceLabel.Title = PointItemManager.ParsePrice(pointItem.CurrentPrice, pointItem.IsMaxUpgrade && pointItem.IsUnlocked);

            ItemBuyElement.Interactable = true;

            if (pointItem.IsUnlocked)
            {
                if (!pointItem.HasUpgrades)
                {
                    ItemBuyElement.gameObject.SetActive(false);
                }
                else if (pointItem.IsMaxUpgrade)
                {
                    ItemBuyElement.Interactable = false;
                    ItemBuyElement.Title = $"Max Level {pointItem.UpgradeCount}";
                }
                else
                {
                    ItemBuyElement.gameObject.SetActive(true);
                    ItemBuyElement.Title = $"Upgrade To Level {pointItem.CurrentUpgradeIndex + 2}/{pointItem.UpgradeCount}";
                }
            }
            else
            {
                ItemBuyElement.gameObject.SetActive(pointItem.CurrentPrice < BitEconomy.PricelessValue);
                ItemBuyElement.Title = "Buy";
            }

            ItemToggleElement.gameObject.SetActive(pointItem.IsUnlocked && pointItem.Equippable);

            if (pointItem.IsEquipped)
            {
                ItemToggleElement.Title = "Unequip";
            }
            else
            {
                ItemToggleElement.Title = "Equip";
            }
        }

        [HideFromIl2Cpp]
        public void ShowPointItems(List<PointItem> pointItems)
        {
            _pointItems = pointItems;

            _palletToPointItem = new();
            _pallets = new();

            foreach (var pointItem in pointItems)
            {
                var pallet = pointItem.Category;

                if (!_pallets.Contains(pallet))
                {
                    _pallets.Add(pallet);
                }

                if (!_palletToPointItem.ContainsKey(pallet))
                {
                    _palletToPointItem[pallet] = new();
                }

                _palletToPointItem[pallet].Add(pointItem);
            }

            _pallets = _pallets
                .OrderBy(x => x)
                .OrderBy(x => !x.StartsWith("Fusion"))
                .ToList();
        }

        protected override void OnDraw()
        {
            if (_catalogTab == CatalogTab.Shop)
            {
                var lockedItems = PointItemManager.GetLockedItems(_sortMode);

                ShowPointItems(lockedItems.ToList());
            }
            else
            {
                var unlockedItems = PointItemManager.GetUnlockedItems(_sortMode);

                ShowPointItems(unlockedItems.ToList());
            }

            DrawBitCount();

            if (BitPalletsPage.isActiveAndEnabled)
            {
                PageArrowsElement.PageCount = GetBitPalletsPageCount();

                DrawBitPallets(PageArrowsElement.PageIndex);
            }

            if (BitResultsPage.isActiveAndEnabled)
            {
                PageArrowsElement.PageCount = GetBitResultsPageCount();

                DrawBitResults(PageArrowsElement.PageIndex);
            }
        }

        private int GetBitPalletsPageCount()
        {
            if (_pallets == null)
            {
                return 0;
            }

            return (int)Mathf.Ceil((float)_pallets.Count / (float)BitPalletElements.Length);
        }

        private int GetBitResultsPageCount()
        {
            if (string.IsNullOrWhiteSpace(_currentPallet) || !_palletToPointItem.ContainsKey(_currentPallet))
            {
                return 0;
            }

            return (int)Mathf.Ceil((float)_palletToPointItem[_currentPallet].Count / (float)BitResultElements.Length);
        }

        private void PopulateBitPalletElements()
        {
            for (var i = 0; i < BitPalletElements.Length; i++)
            {
                var index = i;

                BitPalletElements[i].Do(() =>
                {
                    SelectBitPallet(index);
                });
            }
        }

        private void PopulateBitResultElements()
        {
            for (var i = 0; i < BitResultElements.Length; i++)
            {
                var index = i;

                BitResultElements[i].OnPressed = () =>
                {
                    SelectBitResult(index);
                };
            }
        }

        private void SelectBitPallet(int index)
        {
            if (_pallets == null)
            {
                return;
            }

            var palletIndex = index + PageArrowsElement.PageIndex * BitPalletElements.Length;

            if (palletIndex >= _pallets.Count)
            {
                return;
            }

            _currentPallet = _pallets[palletIndex];

            BrowserPage.SelectSubPage(BitResultsPage);

            PageArrowsElement.PageIndex = 0;
        }

        private void SelectBitResult(int index)
        {
            if (_pointItems == null || _currentPallet == null)
            {
                return;
            }

            var pointItems = _palletToPointItem[_currentPallet];

            var pointItemIndex = index + PageArrowsElement.PageIndex * BitResultElements.Length;

            if (pointItemIndex >= pointItems.Count)
            {
                return;
            }

            var pointItem = pointItems[pointItemIndex];

            ShowPointItem(pointItem);
        }

        private void DrawBitCount()
        {
            CatalogBitCountText.text = $"{PointItemManager.GetBitCount()}";
        }

        private void DrawBitPallets(int page)
        {
            foreach (var element in BitPalletElements)
            {
                element.gameObject.SetActive(false);
            }

            if (_pallets == null)
            {
                return;
            }

            var elementCount = BitPalletElements.Length;
            var startIndex = page * elementCount;

            for (var i = 0; i < elementCount && i + startIndex < _pallets.Count; i++) 
            {
                BitPalletElements[i].Title = _pallets[i + startIndex];
                BitPalletElements[i].gameObject.SetActive(true);
            }
        }

        private void DrawBitResults(int page)
        {
            foreach (var element in BitResultElements)
            {
                element.gameObject.SetActive(false);
            }

            if (_pointItems == null || _currentPallet == null)
            {
                return;
            }

            if (!_palletToPointItem.ContainsKey(_currentPallet))
            {
                return;
            }

            var pointItems = _palletToPointItem[_currentPallet];

            var elementCount = BitResultElements.Length;
            var startIndex = page * elementCount;

            for (var i = 0; i < elementCount && i + startIndex < pointItems.Count; i++)
            {
                BitResultElements[i].ApplyPointItem(pointItems[i + startIndex]);
                BitResultElements[i].gameObject.SetActive(true);
            }
        }
#endif
    }
}