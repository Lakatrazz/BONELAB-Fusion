using Il2CppTMPro;

using UnityEngine;

namespace LabFusion.UI.Styles;

public static class StyleDefaults
{
    public static readonly Color TextColor = Color.white;

    public static readonly TextAlignmentOptions TextAlignment = TextAlignmentOptions.TopLeft;

    public static readonly FontStyles FontStyle = FontStyles.Normal;

    public static readonly float FontSize = 14f;

    public static readonly Color BackgroundColor = Color.clear;
    
    public static readonly float Width = -1f;

    public static readonly float Height = -1f;

    public static readonly float FlexGrow = 0f;

    public static readonly Justify JustifyContent = Justify.Start;

    public static readonly Align AlignItems = Align.Stretch;

    public static readonly Direction Direction = Direction.Column;

    public static readonly Position Position = Position.Relative;

    public static readonly bool AlignSelfStretch = false;
}
