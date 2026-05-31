using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppInterop.Runtime.Attributes;

using Il2CppTMPro;

using LabFusion.UI.Elements;
using LabFusion.UI.Styles;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class TextElementView : UIElementView
    {
#if MELONLOADER
        public TextElementView(IntPtr intPtr) : base(intPtr) { }

        [HideFromIl2Cpp]
        public TMP_Text TextView { get; private set; } = null;

        protected override void OnContentRepainted()
        {
            if (Element is not TextElement textElement)
            {
                return;
            }

            TextView.text = textElement.Text;
        }

        protected override void OnStyleRepainted()
        {
            if (Element is not TextElement textElement)
            {
                return;
            }

            var style = textElement.ResolvedStyle;

            TextView.color = style.TextColor.GetValueOrDefault(StyleDefaults.TextColor);
            TextView.alignment = style.TextAlignment.GetValueOrDefault(StyleDefaults.TextAlignment);

            TextView.font = style.Font;
            TextView.fontStyle = style.FontStyle.GetValueOrDefault(StyleDefaults.FontStyle);
            TextView.fontSize = style.FontSize.GetValueOrDefault(StyleDefaults.FontSize);

            bool hasGradient = style.TextGradient.Keyword.HasValue();
            TextView.enableVertexGradient = hasGradient;

            if (hasGradient)
            {
                var gradient = style.TextGradient.Value;
                TextView.colorGradient = new(gradient.TopLeft, gradient.TopRight, gradient.BottomLeft, gradient.BottomRight);
            }

            var autoSize = style.TextAutoSize.Value;
            bool hasAutoSize = style.TextAutoSize.Keyword.HasValue() && autoSize.Mode == TextAutoSizeMode.BestFit;

            TextView.enableAutoSizing = hasAutoSize;

            if (hasAutoSize)
            {
                TextView.fontSizeMin = autoSize.MinSize;
                TextView.fontSizeMax = autoSize.MaxSize;
            }
        }

        protected override void OnGetReferences()
        {
            TextView = References.MarginsTransform.Find("view_Text").GetComponent<TMP_Text>();
        }
#endif
    }
}