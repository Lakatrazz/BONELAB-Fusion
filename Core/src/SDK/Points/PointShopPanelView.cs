using MelonLoader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using UnhollowerBaseLib.Attributes;

using LabFusion.Utilities;
using LabFusion.UI;
using LabFusion.Extensions;

namespace LabFusion.SDK.Points
{
    [RegisterTypeInIl2Cpp]
    public sealed class PointShopPanelView : MonoBehaviour {
        public enum ActivePanel {
            CATALOG = 0,
            OWNED = 1,
            CONFIRMATION = 2,
            INFORMATION = 3,
            HELP = 4,
        }

        public PointShopPanelView(IntPtr intPtr) : base(intPtr) { }

        private Rigidbody _doorRigidbody;

        private Transform _canvas;

        private Transform _groupItemsRoot;
        private Transform _categorySelectionRoot;
        private Transform _itemButtonsRoot;
        private Transform _arrowButtonsRoot;
        private TMP_Text _pageCountText;

        private Button _catalogButton;
        private Button _ownedButton;

        private Transform _groupInformationRoot;
        private TMP_Text _infoTitle;
        private TMP_Text _infoDescription;
        private TMP_Text _infoPrice;
        private TMP_Text _infoAuthor;
        private TMP_Text _infoVersion;
        private TMP_Text _infoTags;
        private Button _infoBuyConfirm;
        private Button _infoAlreadyOwned;
        private Button _infoGoBack;
        private RawImage _infoPreviewImage;
        private Texture _defaultPreview;

        private Transform _groupWhatIsThisRoot;

        private Button[] _itemButtons;
        private int _itemButtonCount;

        private TMP_Text _bitCountText;

        private Button _toggleButton;
        private TMP_Text _toggleText;

        private Button _sortButton;
        private TMP_Text _sortText;

        private Button _unequipAllButton;

        private int _currentPageIndex = 0;
        private int _catalogPageCount = 0;
        private int _ownedPageCount = 0;
        private ActivePanel _panel = ActivePanel.CATALOG;
        private ActivePanel _lastCatalogPanel = ActivePanel.CATALOG;

        private SortMode _sortMode = SortMode.PRICE;

        private PointItem _targetInfoItem;

        public int PageCount => _panel == ActivePanel.CATALOG ? _catalogPageCount : _panel == ActivePanel.OWNED ? _ownedPageCount : 0;

        public ActivePanel Panel => _panel;

        [HideFromIl2Cpp]
        public IReadOnlyList<PointItem> CatalogItems => PointItemManager.GetLockedItems(_sortMode);

        [HideFromIl2Cpp]
        public IReadOnlyList<PointItem> OwnedItems => PointItemManager.GetUnlockedItems(_sortMode);

        [HideFromIl2Cpp]
        public IReadOnlyList<PointItem> PanelItems => _panel == ActivePanel.CATALOG ? CatalogItems : OwnedItems;

        private void Awake() {
            // Setup the menu
            SetupReferences();
            SetupText();
            SetupButtons();
            SetupArrows();
            SetupInfoPage();

            // Load the first page
            UpdateSortModeText();
            SelectPanel(ActivePanel.CATALOG);
            LoadCatalogPage();

            // Hook into bit update
            PointItemManager.OnBitCountChanged += UpdateBitCountText;
        }

        private void OnDestroy() {
            // Unhook from bit update
            PointItemManager.OnBitCountChanged -= UpdateBitCountText;
        }

        private void SetupReferences() {
            _doorRigidbody = transform.parent.Find("Art/Offset/VendorAtlas/Door Pivot").GetComponent<Rigidbody>();

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
            _infoVersion = _groupInformationRoot.Find("button_Version").GetComponentInChildren<TMP_Text>(true);
            _infoTags = _groupInformationRoot.Find("button_Tags").GetComponentInChildren<TMP_Text>(true);
            _infoBuyConfirm = _groupInformationRoot.Find("button_BuyConfirm").GetComponentInChildren<Button>(true);
            _infoAlreadyOwned = _groupInformationRoot.Find("button_AlreadyOwned").GetComponentInChildren<Button>(true);
            _infoGoBack = _groupInformationRoot.Find("button_goBack").GetComponentInChildren<Button>(true);
            _infoPreviewImage = _groupInformationRoot.Find("image_IconPreview").GetComponentInChildren<RawImage>(true);
            _defaultPreview = _infoPreviewImage.texture;

            _toggleButton = _groupInformationRoot.Find("button_Toggle").GetComponent<Button>();
            _toggleButton.onClick.AddListener((UnityAction)(() => {
                ConfirmToggle();
            }));
            _toggleText = _toggleButton.GetComponentInChildren<TMP_Text>(true);

            _sortButton = _canvas.Find("button_SortBy").GetComponent<Button>();
            _sortButton.onClick.AddListener((UnityAction)(() => {
                SelectSort();
            }));
            _sortText = _sortButton.GetComponentInChildren<TMP_Text>(true);

            _unequipAllButton = _canvas.Find("button_UnequipAll").GetComponent<Button>();
            _unequipAllButton.onClick.AddListener((UnityAction)(() => {
                PointItemManager.UnequipAll();

                SelectPanel(ActivePanel.OWNED);
                LoadCatalogPage();
            }));

            _bitCountText = _canvas.Find("button_bitCount").GetComponentInChildren<TMP_Text>(true);

            _groupWhatIsThisRoot = _canvas.Find("group_WhatIsThis");

            var helpButton = _canvas.Find("button_Help").GetComponent<Button>();
            helpButton.onClick.AddListener((UnityAction)(() =>
            {
                SelectPanel(ActivePanel.HELP);
            }));

            var helpGoBack = _groupWhatIsThisRoot.Find("button_goBack").GetComponent<Button>();
            helpGoBack.onClick.AddListener((UnityAction)(() =>
            {
                SelectPanel(_lastCatalogPanel);
                LoadCatalogPage();
            }));
        }

        private void SetupText() {
            foreach (var text in gameObject.GetComponentsInChildren<TMP_Text>(true)) {
                text.font = PersistentAssetCreator.Font;
            }
        }

        private void SetupButtons() {
            // Get the panel buttons
            _catalogButton = _categorySelectionRoot.Find("button_catalog").GetComponent<Button>();
            _catalogButton.onClick.AddListener((UnityAction)(() => {
                _currentPageIndex = 0;
                SelectPanel(ActivePanel.CATALOG);
                LoadCatalogPage();
            }));

            _ownedButton = _categorySelectionRoot.Find("button_owned").GetComponent<Button>();
            _ownedButton.onClick.AddListener((UnityAction)(() => {
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
                    var interactor = collider.gameObject.AddComponent<FusionUITrigger>();
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
                    _groupWhatIsThisRoot.gameObject.SetActive(false);

                    _lastCatalogPanel = panel;
                    break;
                case ActivePanel.CONFIRMATION:
                case ActivePanel.INFORMATION:
                    _groupItemsRoot.gameObject.SetActive(false);
                    _groupInformationRoot.gameObject.SetActive(true);
                    _groupWhatIsThisRoot.gameObject.SetActive(false);
                    break;
                case ActivePanel.HELP:
                    _groupItemsRoot.gameObject.SetActive(false);
                    _groupInformationRoot.gameObject.SetActive(false);
                    _groupWhatIsThisRoot.gameObject.SetActive(true);
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
            _pageCountText.text = $"Page {_currentPageIndex + 1} out of {Mathf.Max(1, PageCount)}";
        }

        private void UpdateBitCountText() {
            _bitCountText.text = PointItemManager.GetBitCount().ToString();
        }

        [HideFromIl2Cpp]
        private void UpdateToggleText(PointItem item) {
            _toggleText.text = item.IsEquipped ? "Unequip" : "Equip";
        }

        private void LoadCatalogPage() {
            // Push updates
            switch (_panel) {
                case ActivePanel.CATALOG:
                    PushCatalogUpdate();

                    _catalogButton.gameObject.SetActive(false);
                    _ownedButton.gameObject.SetActive(true);
                    break;
                case ActivePanel.OWNED:
                    PushOwnedUpdate();

                    _catalogButton.gameObject.SetActive(true);
                    _ownedButton.gameObject.SetActive(false);
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
            string tag = item.MainTag;

            if (Panel == ActivePanel.CATALOG)
                text.text = $"{item.Title} - {item.AdjustedPrice} Bits ({tag})";
            else
                text.text = $"{item.Title} ({tag})";

            text.color = PointItemManager.ParseColor(item.Rarity);
        }

        [HideFromIl2Cpp]
        private void LoadInfoPage(PointItem item) {
            _infoTitle.text = item.Title;
            _infoTitle.color = PointItemManager.ParseColor(item.Rarity);
            _infoDescription.text = item.Description;
            _infoPrice.text = $"{item.AdjustedPrice} Bits";
            _infoAuthor.text = item.Author;
            _infoVersion.text = item.Version;

            if (item.Tags == null || item.Tags.Length <= 0)
                _infoTags.text = "Misc";
            else {
                string tags = "";
                bool isFirst = true;

                foreach (var tag in item.Tags) {
                    if (isFirst)
                        tags = tag;
                    else
                        tags += $", {tag}";

                    isFirst = false;
                }

                _infoTags.text = tags;
            }
            
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

                    if (item.CanEquip) {
                        _toggleButton.gameObject.SetActive(true);
                        UpdateToggleText(item);
                    }
                    else {
                        _toggleButton.gameObject.SetActive(false);
                    }
                    break;
                case ActivePanel.CONFIRMATION:
                    _infoBuyConfirm.gameObject.SetActive(true);
                    _infoAlreadyOwned.gameObject.SetActive(false);

                    _toggleButton.gameObject.SetActive(false);
                    break;
            }

            // Update text
            UpdateBitCountText();
        }
        
        private void SelectSort() {
            int currentSort = (int)_sortMode;
            currentSort++;

            if (currentSort >= (int)SortMode.LAST_SORT)
                currentSort = 0;

            _sortMode = (SortMode)currentSort;
            UpdateSortModeText();

            if (_panel == ActivePanel.CATALOG || _panel == ActivePanel.OWNED) {
                _currentPageIndex = 0;
                LoadCatalogPage();
            }
        }

        private void UpdateSortModeText() {
            _sortText.text = $"Sort By: {_sortMode}";
        }

        private void ConfirmToggle() {
            // Make sure we have a target
            if (_targetInfoItem == null)
                return;

            // Check the current state
            // Unequip
            if (_targetInfoItem.IsEquipped) {
                PointItemManager.SetEquipped(_targetInfoItem, false);

                FusionAudio.Play3D(transform.position, FusionContentLoader.UnequipItem);

                _doorRigidbody.AddRelativeTorque(Vector3Extensions.right * 30f, ForceMode.Impulse);
            }
            // Equip
            else {
                PointItemManager.SetEquipped(_targetInfoItem, true);

                FusionAudio.Play3D(transform.position, FusionContentLoader.EquipItem);

                _doorRigidbody.AddRelativeTorque(Vector3Extensions.left * 30f, ForceMode.Impulse);
            }

            // Update text
            UpdateToggleText(_targetInfoItem);
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

                FusionAudio.Play3D(transform.position, FusionContentLoader.PurchaseSuccess, 1f);
            }
            // Failure
            else {
                FusionAudio.Play3D(transform.position, FusionContentLoader.PurchaseFailure, 1f);
            }
        }

        private int GetItemIndex(int button) {
            return button + (_currentPageIndex * _itemButtonCount);
        }
    }
}
