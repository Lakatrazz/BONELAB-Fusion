using UnityEngine;

using LabFusion.Extensions;
using LabFusion.UI.Resources;

using Il2CppTMPro;

namespace LabFusion.UI.Styles;

public class Style : IReadOnlyStyle
{
    public StyleValue<Color> TextColor 
    { 
        get => GetProperty<Color>(CommonStyleProperties.TextColor); 
        set => SetProperty(CommonStyleProperties.TextColor, value); 
    }

    public StyleValue<VertexColors> TextGradient
    {
        get => GetProperty<VertexColors>(CommonStyleProperties.TextGradient);
        set => SetProperty(CommonStyleProperties.TextGradient, value);
    }

    public StyleValue<TextAlignmentOptions> TextAlignment
    {
        get => GetProperty<TextAlignmentOptions>(CommonStyleProperties.TextAlignment);
        set => SetProperty(CommonStyleProperties.TextAlignment, value);
    }

    public StyleValue<TextAutoSize> TextAutoSize
    {
        get => GetProperty<TextAutoSize>(CommonStyleProperties.TextAutoSize);
        set => SetProperty(CommonStyleProperties.TextAutoSize, value);
    }

    public StyleValue<TextOutline> TextOutline
    {
        get => GetProperty<TextOutline>(CommonStyleProperties.TextOutline);
        set => SetProperty(CommonStyleProperties.TextOutline, value);
    }

    public StyleValue<TextShadow> TextShadow
    {
        get => GetProperty<TextShadow>(CommonStyleProperties.TextShadow);
        set => SetProperty(CommonStyleProperties.TextShadow, value);
    }

    public StyleValue<TextGlow> TextGlow
    {
        get => GetProperty<TextGlow>(CommonStyleProperties.TextGlow);
        set => SetProperty(CommonStyleProperties.TextGlow, value);
    }

    public StyleValue<TMP_FontAsset> Font
    {
        get => GetProperty<TMP_FontAsset>(CommonStyleProperties.Font);
        set => SetProperty(CommonStyleProperties.Font, value);
    }

    public StyleValue<FontStyles> FontStyle
    {
        get => GetProperty<FontStyles>(CommonStyleProperties.FontStyle);
        set => SetProperty(CommonStyleProperties.FontStyle, value);
    }

    public StyleValue<Length> FontSize
    {
        get => GetProperty<Length>(CommonStyleProperties.FontSize);
        set => SetProperty(CommonStyleProperties.FontSize, value);
    }

    public StyleValue<Color> BackgroundColor
    {
        get => GetProperty<Color>(CommonStyleProperties.BackgroundColor);
        set => SetProperty(CommonStyleProperties.BackgroundColor, value);
    }

    public StyleValue<Texture> BackgroundImage
    {
        get => GetProperty<Texture>(CommonStyleProperties.BackgroundImage);
        set => SetProperty(CommonStyleProperties.BackgroundImage, value);
    }

    public StyleValue<float> Width
    {
        get => GetProperty<float>(CommonStyleProperties.Width);
        set => SetProperty(CommonStyleProperties.Width, value);
    }

    public StyleValue<float> Height
    {
        get => GetProperty<float>(CommonStyleProperties.Height);
        set => SetProperty(CommonStyleProperties.Height, value);
    }

    public StyleValue<Direction> Direction
    {
        get => GetProperty<Direction>(CommonStyleProperties.Direction);
        set => SetProperty(CommonStyleProperties.Direction, value);
    }

    public StyleValue<Position> Position
    {
        get => GetProperty<Position>(CommonStyleProperties.Position);
        set => SetProperty(CommonStyleProperties.Position, value);
    }

    public StyleValue<float> FlexGrow
    {
        get => GetProperty<float>(CommonStyleProperties.FlexGrow);
        set => SetProperty(CommonStyleProperties.FlexGrow, value);
    }

    public StyleValue<Justify> JustifyContent
    {
        get => GetProperty<Justify>(CommonStyleProperties.JustifyContent);
        set => SetProperty(CommonStyleProperties.JustifyContent, value);
    }

    public StyleValue<Align> AlignItems
    {
        get => GetProperty<Align>(CommonStyleProperties.AlignItems);
        set => SetProperty(CommonStyleProperties.AlignItems, value);
    }

    public StyleValue<bool> AlignSelfStretch
    {
        get => GetProperty<bool>(CommonStyleProperties.AlignSelfStretch);
        set => SetProperty(CommonStyleProperties.AlignSelfStretch, value);
    }

    public StyleValue<BorderOffsets> Margins
    {
        get => GetProperty<BorderOffsets>(CommonStyleProperties.Margins);
        set => SetProperty(CommonStyleProperties.Margins, value);
    }

    public StyleValue<BorderOffsets> Padding
    {
        get => GetProperty<BorderOffsets>(CommonStyleProperties.Padding);
        set => SetProperty(CommonStyleProperties.Padding, value);
    }

    public StyleValue<Vector2> AbsoluteOffset
    {
        get => GetProperty<Vector2>(CommonStyleProperties.AbsoluteOffset);
        set => SetProperty(CommonStyleProperties.AbsoluteOffset, value);
    }

    public event Action StyleChanged;

    public IReadOnlyDictionary<string, IStyleValue> SetProperties => _setProperties;

    private readonly Dictionary<string, IStyleValue> _setProperties = new();

    public Style() { }

    public Style(Style other)
    {
        _setProperties = new(other._setProperties.Count);

        foreach (var pair in other._setProperties)
        {
            _setProperties[pair.Key] = pair.Value.Clone();
        }
    }

    public Style(Action changeCallback)
    {
        StyleChanged += changeCallback;
    }

    public StyleValue<T> GetProperty<T>(string propertyName)
    {
        if (_setProperties.TryGetValue(propertyName, out var value))
        {
            return (StyleValue<T>)value;
        }

        return StyleKeyword.Null;
    }

    public void SetProperty(string propertyName, IStyleValue value)
    {
        if (value.Keyword == StyleKeyword.Null)
        {
            RemoveProperty(propertyName);
            return;
        }

        _setProperties[propertyName] = value;

        NotifyStyleChanged();
    }

    public void RemoveProperty(string propertyName) 
    { 
        _setProperties.Remove(propertyName);

        NotifyStyleChanged();
    }

    public bool HasSetProperty(string propertyName) => _setProperties.ContainsKey(propertyName);

    private void NotifyStyleChanged() => StyleChanged?.InvokeSafe("executing StyleChanged event");
}
