using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppInterop.Runtime.Attributes;

using Il2CppTMPro;

using LabFusion.UI.Elements;
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

        protected override void OnRepaintedElement(UIElement element)
        {
            base.OnRepaintedElement(element);

            if (element is not TextElement textElement)
            {
                return;
            }

            TextView.text = textElement.Text;

            var style = textElement.Style;

            TextView.color = style.TextColor;

            TextView.font = style.Font;
            TextView.fontSize = style.FontSize ?? 14f;

            bool hasGradient = style.TextGradient.HasValue;
            TextView.enableVertexGradient = hasGradient;

            if (hasGradient)
            {
                var gradient = style.TextGradient.Value;
                TextView.colorGradient = new(gradient.TopLeft, gradient.TopRight, gradient.BottomLeft, gradient.BottomRight);
            }
        }

        protected override void OnGetReferences()
        {
            TextView = References.MarginsTransform.Find("view_Text").GetComponent<TMP_Text>();
        }
#endif
    }
}