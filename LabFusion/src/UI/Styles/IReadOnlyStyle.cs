using Il2CppTMPro;

using UnityEngine;

namespace LabFusion.UI.Styles;

public interface IReadOnlyStyle
{
    StyleValue<Color> TextColor { get; }

    StyleValue<VertexColors> TextGradient { get; }

    StyleValue<TextAlignmentOptions> TextAlignment { get; }

    StyleValue<TMP_FontAsset> Font { get; }

    StyleValue<FontStyles> FontStyle { get; }

    StyleValue<float> FontSize { get; }

    StyleValue<Color> BackgroundColor { get; }

    StyleValue<Texture> BackgroundImage { get; }

    StyleValue<float> Width { get; }

    StyleValue<float> Height { get; }

    StyleValue<Direction> Direction { get; }

    StyleValue<Position> Position { get; }

    StyleValue<float> FlexGrow { get; }

    StyleValue<Justify> JustifyContent { get; }

    StyleValue<Align> AlignItems { get; }

    StyleValue<bool> AlignSelfStretch { get; }
    
    StyleValue<BorderOffsets> Margins { get; }
    
    StyleValue<BorderOffsets> Padding { get; }

    StyleValue<Vector2> AbsoluteOffset { get; }

    IReadOnlyDictionary<string, IStyleValue> SetProperties { get; }
}
