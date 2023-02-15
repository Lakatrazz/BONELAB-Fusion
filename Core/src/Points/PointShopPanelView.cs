using MelonLoader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

using LabFusion.Points;

using TMPro;

using UnhollowerBaseLib.Attributes;
using SLZ.UI;
using LabFusion.Utilities;

namespace LabFusion.Points
{
    [RegisterTypeInIl2Cpp]
    public sealed class PointShopPanelView : MonoBehaviour {
        public enum ActivePanel {
            CATALOG = 0,
            OWNED = 1,
            CONFIRMATION = 2,
            INFORMATION = 3,
        }

        public PointShopPanelView(IntPtr intPtr) : base(intPtr) { }

        private Transform _canvas;

        private Transform _groupItemsRoot;
        private Transform _categorySelectionRoot;
        private Transform _itemButtonsRoot;
        private Transform _arrowButtonsRoot;
        private TMP_Text _pageCountText;

        private Transform _groupInformationRoot;
        private TMP_Text _infoTitle;
        private TMP_Text _infoDescription;
        private TMP_Text _infoPrice;
        private TMP_Text _infoAuthor;
        private Button _infoBuyConfirm;
        private Button _infoAlreadyOwned;
        private Button _infoGoBack;
        private RawImage _infoPreviewImage;
        private Texture _defaultPreview;

        private Button[] _itemButtons;
        private int _itemButtonCount;

        private TMP_Text _bitCountText;

        private int _currentPageIndex = 0;
        private int _catalogPageCount = 0;
        private int _ownedPageCount = 0;
        private ActivePanel _panel = ActivePanel.CATALOG;
        private ActivePanel _lastCatalogPanel = ActivePanel.CATALOG;

        private PointItem _targetInfoItem;

        public int PageCount => _panel == ActivePanel.CATALOG ? _catalogPageCount : _panel == ActivePanel.OWNED ? _ownedPageCount : 0;

        public ActivePanel Panel => _panel;

        [HideFromIl2Cpp]
        public IReadOnlyList<PointItem> CatalogItems => PointItemManager.GetLockedItems();

        [HideFromIl2Cpp]
        public IReadOnlyList<PointItem> OwnedItems => PointItemManager.GetUnlockedItems();

        [HideFromIl2Cpp]
        public IReadOnlyList<PointItem> PanelItems => _panel == ActivePanel.CATALOG ? CatalogItems : OwnedItems;

        public void Awake() {
            // Setup the menu
            SetupReferences();
            SetupText();
            SetupButtons();
            SetupArrows();
            SetupInfoPage();

            // Load the first page
            SelectPanel(ActivePanel.CATALOG);
            LoadCatalogPage();
        }

        private void SetupReferences() {
            _canvas = transform.Find("CANVAS");

            _groupItemsRoot = _canvas.Find("group_Items");
            _categorySelectionRoot = _groupItemsRoot.Find("category_Selection");
            _itemButtonsRoot = _groupItemsRoot.Find("item_Buttons");
            _arrowButtonsRoot = _groupItemsRoot.Find("arrow_Buttons");
            _pageCountText = _groupItemsRoot.Find("button_pageCount").GetComponentInChildren<TMP_Text>(true);

            _groupInformationRoot = _canvas.Find("group_Information");
            _infoTitle = _groupInformationRoot.Find("button_Title").GetComponentInChildren<TMP_Text>(true);
            _infoDescription = _groupInformationRoot.Find("button_Description").GetComponentInChildren<TMP_Text>(true);
            _infoPrice = _groupInformationRoot.Find("button_Price").GetComponentInChildren<TMP_Text>(true);
            _infoAuthor = _groupInformationRoot.Find("button_Author").GetComponentInChildren<TMP_Text>(true);
            _infoBuyConfirm = _groupInformationRoot.Find("button_BuyConfirm").GetComponentInChildren<Button>(true);
            _infoAlreadyOwned = _groupInformationRoot.Find("button_AlreadyOwned").GetComponentInChildren<Button>(true);
            _infoGoBack = _groupInformationRoot.Find("button_goBack").GetComponentInChildren<Button>(true);
            _infoPreviewImage = _groupInformationRoot.Find("image_IconPreview").GetComponentInChildren<RawImage>(true);
            _defaultPreview = _infoPreviewImage.texture;

            _bitCountText = _canvas.Find("button_bitCount").GetComponentInChildren<TMP_Text>();
        }

        private void SetupText() {
            foreach (var text in gameObject.GetComponentsInChildren<TMP_Text>(true)) {
                text.font = PersistentAssetCreator.Font;
            }
        }

        private void SetupButtons() {
            // Get the panel buttons
            var catalogButton = _categorySelectionRoot.Find("button_catalog").GetComponent<Button>();
            catalogButton.onClick.AddListener((UnityAction)(() => {
                _currentPageIndex = 0;
                SelectPanel(ActivePanel.CATALOG);
                LoadCatalogPage();
            }));

            var ownedButton = _categorySelectionRoot.Find("button_owned").GetComponent<Button>();
            ownedButton.onClick.AddListener((UnityAction)(() => {
                _currentPageIndex = 0;
                SelectPanel(ActivePanel.OWNED);
                LoadCatalogPage();
            }));

            // Get the buttons
            _itemButtons = _itemButtonsRoot.GetComponentsInChildren<Button>(true);
            _itemButtonCount = _itemButtons.Length;

            for (var i = 0; i < _itemButtonCount; i++) {
                int button = i;

                // Add click listener
                _itemButtons[i].onClick.AddListener((UnityAction)(() => {
                    SelectItem(button);
                }));
            }

            // Add clicking events to every button
            foreach (var button in transform.GetComponentsInChildren<Button>(true)) {
                var collider = button.GetComponentInChildren<Collider>(true);
                if (collider != null)
                {
                    var interactor = collider.gameObject.AddComponent<PointShopUITrigger>();
                    interactor.button = button;
                }
            }
        }

        private void SetupArrows() {
            // Setup the arrows
            _arrowButtonsRoot.Find("button_lastPage").GetComponent<Button>().onClick.AddListener((UnityAction)(() =>
            {
                LastPage();
            }));
            _arrowButtonsRoot.Find("button_nextPage").GetComponent<Button>().onClick.AddListener((UnityAction)(() =>
            {
                NextPage();
            }));
        }

        private void SetupInfoPage() {
            _infoGoBack.onClick.AddListener((UnityAction)(() => {
                SelectPanel(_lastCatalogPanel);
                LoadCatalogPage();
            }));
            _infoBuyConfirm.onClick.AddListener((UnityAction)(() => {
                ConfirmBuy();
            }));
        }

        private void PushCatalogUpdate()
        {
            // Get page count
            if (CatalogItems.Count <= 0) {
                _catalogPageCount = 0;
            }
            else {
                _catalogPageCount = (int)Math.Ceiling((double)CatalogItems.Count / (double)_itemButtonCount);
            }
        }

        private void PushOwnedUpdate() {
            // Get page count
            if (OwnedItems.Count <= 0)
            {
                _ownedPageCount = 0;
            }
            else
            {
                _ownedPageCount = (int)Math.Ceiling((double)OwnedItems.Count / (double)_itemButtonCount);
            }
        }

        private void SelectItem(int index) {
            var itemIndex = GetItemIndex(index);
            if (PanelItems.Count <= itemIndex)
                return;

            PointItem item = PanelItems[itemIndex];

            switch (Panel) {
                case ActivePanel.CATALOG:
                    SelectPanel(ActivePanel.CONFIRMATION);
                    LoadInfoPage(item);
                    break;
                case ActivePanel.OWNED:
                    SelectPanel(ActivePanel.INFORMATION);
                    LoadInfoPage(item);
                    break;
            }
        }

        private void SelectPanel(ActivePanel panel) {
            _panel = panel;

            switch (panel) {
                default:
                case ActivePanel.CATALOG:
                case ActivePanel.OWNED:
                    _groupItemsRoot.gameObject.SetActive(true);
                    _groupInformationRoot.gameObject.SetActive(false);

                    _lastCatalogPanel = panel;
                    break;
                case ActivePanel.CONFIRMATION:
                case ActivePanel.INFORMATION:
                    _groupItemsRoot.gameObject.SetActive(false);
                    _groupInformationRoot.gameObject.SetActive(true);
                    break;
            }
        }

        public void NextPage() {
            _currentPageIndex++;

            if (_currentPageIndex >= PageCount)
                _currentPageIndex = PageCount - 1;

            LoadCatalogPage();
        }

        public void LastPage() {
            _currentPageIndex--;

            if (_currentPageIndex < 0)
                _currentPageIndex = 0;

            LoadCatalogPage();
        }

        private void UpdatePageCountText() {
            _pageCountText.text = $"Page {_currentPageIndex + 1} out of {PageCount}";
        }

        private void UpdateBitCountText() {
            _bitCountText.text = PointItemManager.GetBitCount().ToString();
        }

        private void LoadCatalogPage() {
            PointItemManager.SortItems();

            // Push updates
            switch (_panel) {
                case ActivePanel.CATALOG:
                    PushCatalogUpdate();
                    break;
                case ActivePanel.OWNED:
                    PushOwnedUpdate();
                    break;
            }

            _currentPageIndex = Mathf.Clamp(_currentPageIndex, 0, PageCount);

            // Loop through every button
            for (var i = 0; i < _itemButtonCount; i++) {
                var button = _itemButtons[i];
                var itemIndex = GetItemIndex(i);

                if (PanelItems.Count <= itemIndex) {
                    button.gameObject.SetActive(false);
                    continue;
                }

                button.gameObject.SetActive(true);

                LoadItem(button, PanelItems[itemIndex]);
            }

            // Update text
            UpdatePageCountText();
            UpdateBitCountText();
        }

        [HideFromIl2Cpp]
        private void LoadItem(Button button, PointItem item) {
            var text = button.GetComponentInChildren<TMP_Text>(true);

            if (Panel == ActivePanel.CATALOG)
                text.text = $"{item.Title} - {item.Price} Bits";
            else
                text.text = item.Title;

            text.color = PointItemManager.ParseColor(item.Rarity);
        }

        [HideFromIl2Cpp]
        private void LoadInfoPage(PointItem item) {
            _infoTitle.text = item.Title;
            _infoTitle.color = PointItemManager.ParseColor(item.Rarity);
            _infoDescription.text = item.Description;
            _infoPrice.text = $"{item.Price} Bits";
            _infoAuthor.text = item.Author;
            
            if (item.PreviewImage != null) {
                _infoPreviewImage.texture = item.PreviewImage;
                _infoPreviewImage.gameObject.SetActive(true);
            }
            else {
                _infoPreviewImage.texture = _defaultPreview;
                _infoPreviewImage.gameObject.SetActive(true);
            }

            _targetInfoItem = item;

            switch (Panel) {
                case ActivePanel.INFORMATION:
                    _infoBuyConfirm.gameObject.SetActive(false);
                    _infoAlreadyOwned.gameObject.SetActive(true);
                    break;
                case ActivePanel.CONFIRMATION:
                    _infoBuyConfirm.gameObject.SetActive(true);
                    _infoAlreadyOwned.gameObject.SetActive(false);
                    break;
            }

            // Update text
            UpdateBitCountText();
        }
        
        private void ConfirmBuy() {
            // Make sure we have a target
            if (_targetInfoItem == null)
                return;

            // Try buying the item
            // Check for success
            if (PointItemManager.TryBuyItem(_targetInfoItem)) {
                SelectPanel(_lastCatalogPanel);
                LoadCatalogPage();

                FusionAudio.Play3D(transform.position, FusionBundleLoader.PurchaseSuccess, 1f);
            }
            // Failure
            else {
                FusionAudio.Play3D(transform.position, FusionBundleLoader.PurchaseFailure, 1f);
            }
        }

        private int GetItemIndex(int button) {
            return button + (_currentPageIndex * _itemButtonCount);
        }
    }
}
