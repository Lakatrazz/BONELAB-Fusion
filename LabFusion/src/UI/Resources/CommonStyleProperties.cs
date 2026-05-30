namespace LabFusion.UI.Resources;

public static class CommonStyleProperties
{
    public static readonly string TextColor = "text-color";
    public static readonly string TextAlignment = "text-alignment";
    public static readonly string TextGradient = "text-gradient";

    public static readonly string Font = "font";
    public static readonly string FontStyle = "font-style";
    public static readonly string FontSize = "font-size";

    public static readonly string BackgroundColor = "background-color";
    public static readonly string BackgroundImage = "background-image";

    public static readonly string Width = "width";
    public static readonly string Height = "height";

    public static readonly string Direction = "direction";
    public static readonly string Position = "position";

    public static readonly string FlexGrow = "flex-grow";

    public static readonly string JustifyContent = "justify-content";
    public static readonly string AlignItems = "align-items";
    public static readonly string AlignSelfStretch = "align-self-stretch";

    public static readonly string Margins = "margins";
    public static readonly string Padding = "padding";

    public static readonly string AbsoluteOffset = "absolute-offset";

    public static readonly List<string> InheritedProperties = new()
    {
        TextColor,
        TextAlignment,
        TextGradient,

        Font,
        FontStyle,
        FontSize
    };
}
