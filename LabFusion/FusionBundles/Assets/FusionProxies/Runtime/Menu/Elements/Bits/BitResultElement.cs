#if MELONLOADER
using Il2CppInterop.Runtime.Attributes;
using Il2CppTMPro;

using LabFusion.SDK.Points;

using MelonLoader;

using UnityEngine;
using UnityEngine.UI;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class BitResultElement : MenuElement
    {
#if MELONLOADER
        public BitResultElement(IntPtr intPtr) : base(intPtr) { }

        public RawImage Preview { get; set; } = null;

        public TMP_Text ItemNameText { get; set; } = null;
        public TMP_Text ItemTagText { get; set; } = null;
        public TMP_Text ItemPriceText { get; set; } = null;

        private Texture _defaultPreview = null;

        public Action OnPressed;

        private bool _hasReferences = false;

        public void Awake()
        {
            GetReferences();
        }
        
        public void GetReferences()
        {
            if (_hasReferences) 
            { 
                return; 
            }

            Preview = transform.Find("icon_Background/icon_Preview").GetComponent<RawImage>();

            _defaultPreview = Preview.texture;

            var infoLayout = transform.Find("layout_Info");
            ItemNameText = infoLayout.Find("text_ItemName").GetComponent<TMP_Text>();
            ItemTagText = infoLayout.Find("text_ItemTag").GetComponent<TMP_Text>();
            ItemPriceText = infoLayout.Find("text_ItemPrice").GetComponent<TMP_Text>();

            _hasReferences = true;
        }

        public void Press()
        {
            OnPressed?.Invoke();
        }

        [HideFromIl2Cpp]
        public void ApplyPointItem(PointItem item)
        {
            GetReferences();

            ItemNameText.text = item.Title;
            ItemNameText.color = PointItemManager.ParseColor(item.Rarity);

            ItemTagText.text = item.MainTag;

            ItemPriceText.text = PointItemManager.ParsePrice(item.Price, item.IsUnlocked);

            Preview.texture = _defaultPreview;

            item.LoadPreviewIcon((texture) =>
            {
                if (Preview == null)
                {
                    return;
                }

                Preview.texture = texture;
            });
        }
#else
        public void Press()
        {

        }
#endif
    }
}