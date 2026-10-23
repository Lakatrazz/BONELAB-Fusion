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

        public static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
        public static readonly int OutlineWidthID = Shader.PropertyToID("_OutlineWidth");
        public static readonly int OutlineSoftnessID = Shader.PropertyToID("_OutlineSoftness");

        public static readonly int UnderlayColorID = Shader.PropertyToID("_UnderlayColor");
        public static readonly int UnderlayOffsetXID = Shader.PropertyToID("_UnderlayOffsetX");
        public static readonly int UnderlayOffsetYID = Shader.PropertyToID("_UnderlayOFfsetY");
        public static readonly int UnderlayDilateID = Shader.PropertyToID("_UnderlayDilate");
        public static readonly int UnderlaySoftnessID = Shader.PropertyToID("_UnderlaySoftness");

        public static readonly int GlowColorID = Shader.PropertyToID("_GlowColor");
        public static readonly int GlowOffsetID = Shader.PropertyToID("_GlowOffset");
        public static readonly int GlowInnerID = Shader.PropertyToID("_GlowInner");
        public static readonly int GlowOuterID = Shader.PropertyToID("_GlowOuter");
        public static readonly int GlowPowerID = Shader.PropertyToID("_GlowPower");

        [HideFromIl2Cpp]
        public TMP_Text TextView { get; private set; } = null;

        [HideFromIl2Cpp]
        public bool IsMaterialOverridden { get; private set; } = false;

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

            bool isFontChanged = TextView.font != style.Font;

            if (isFontChanged)
            {
                IsMaterialOverridden = false;
            }

            TextView.font = style.Font;
            TextView.fontStyle = style.FontStyle.GetValueOrDefault(StyleDefaults.FontStyle);
            TextView.fontSize = style.FontSize.GetValueOrDefault(StyleDefaults.FontSize);

            bool hasGradient = style.TextGradient.HasValue();
            TextView.enableVertexGradient = hasGradient;

            if (hasGradient)
            {
                var gradient = style.TextGradient.Value;
                TextView.colorGradient = new(gradient.TopLeft, gradient.TopRight, gradient.BottomLeft, gradient.BottomRight);
            }

            var autoSize = style.TextAutoSize.Value;
            bool hasAutoSize = style.TextAutoSize.HasValue() && autoSize.Mode == TextAutoSizeMode.BestFit;

            TextView.enableAutoSizing = hasAutoSize;

            if (hasAutoSize)
            {
                TextView.fontSizeMin = autoSize.MinSize;
                TextView.fontSizeMax = autoSize.MaxSize;
            }

            bool hasOutline = style.TextOutline.HasValue();

            bool hasShadow = style.TextShadow.HasValue();

            bool hasGlow = style.TextGlow.HasValue();

            bool hasExtraMaterialProperties = hasOutline || hasShadow || hasGlow;
            TextView.extraPadding = hasExtraMaterialProperties;

            if (hasExtraMaterialProperties)
            {
                IsMaterialOverridden = true;
            }

            if (IsMaterialOverridden)
            {
                var fontMaterial = TextView.fontMaterial;

                SetOutline(fontMaterial, hasOutline ? style.TextOutline.Value : TextOutline.None);
                SetShadow(fontMaterial, hasShadow ? style.TextShadow.Value : TextShadow.None);
                SetGlow(fontMaterial, hasGlow ? style.TextGlow.Value : TextGlow.None);
            }
        }

        private static void SetOutline(Material material, TextOutline outline)
        {
            material.SetColor(OutlineColorID, outline.Color);
            material.SetFloat(OutlineWidthID, outline.Width);
            material.SetFloat(OutlineSoftnessID, outline.Softness);
        }

        private static void SetShadow(Material material, TextShadow shadow)
        {
            material.SetColor(UnderlayColorID, shadow.Color);
            material.SetFloat(UnderlayOffsetXID, shadow.OffsetX);
            material.SetFloat(UnderlayOffsetYID, shadow.OffsetY);
            material.SetFloat(UnderlayDilateID, shadow.Dilate);
            material.SetFloat(UnderlaySoftnessID, shadow.Softness);
        }

        private static void SetGlow(Material material, TextGlow glow)
        {
            material.SetColor(GlowColorID, glow.Color);
            material.SetFloat(GlowOffsetID, glow.Offset);
            material.SetFloat(GlowInnerID, glow.Inner);
            material.SetFloat(GlowOuterID, glow.Outer);
            material.SetFloat(GlowPowerID, glow.Power);
        }

        protected override void OnGetReferences()
        {
            TextView = References.MarginsTransform.Find("view_Text").GetComponent<TMP_Text>();
        }
#endif
    }
}