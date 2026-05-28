using UnityEngine;

using LabFusion.Extensions;
using LabFusion.UI.Resources;

using Il2CppTMPro;

namespace LabFusion.UI.Styles;

public class Style : IReadOnlyStyle
{
    public StyleValue<Color> TextColor 
    { 
        get => GetProperty<Color>(CommonProperties.TextColor); 
        set => SetProperty(CommonProperties.TextColor, value); 
    }

    public StyleValue<VertexColors> TextGradient
    {
        get => GetProperty<VertexColors>(CommonProperties.TextGradient);
        set => SetProperty(CommonProperties.TextGradient, value);
    }

    public StyleValue<TMP_FontAsset> Font
    {
        get => GetProperty<TMP_FontAsset>(CommonProperties.Font);
        set => SetProperty(CommonProperties.Font, value);
    }

    public StyleValue<float> FontSize
    {
        get => GetProperty<float>(CommonProperties.FontSize);
        set => SetProperty(CommonProperties.FontSize, value);
    }

    public StyleValue<Color> BackgroundColor
    {
        get => GetProperty<Color>(CommonProperties.BackgroundColor);
        set => SetProperty(CommonProperties.BackgroundColor, value);
    }

    public StyleValue<Texture> BackgroundImage
    {
        get => GetProperty<Texture>(CommonProperties.BackgroundImage);
        set => SetProperty(CommonProperties.BackgroundImage, value);
    }

    public StyleValue<float> Width
    {
        get => GetProperty<float>(CommonProperties.Width);
        set => SetProperty(CommonProperties.Width, value);
    }

    public StyleValue<float> Height
    {
        get => GetProperty<float>(CommonProperties.Height);
        set => SetProperty(CommonProperties.Height, value);
    }

    public StyleValue<Direction> Direction
    {
        get => GetProperty<Direction>(CommonProperties.Direction);
        set => SetProperty(CommonProperties.Direction, value);
    }

    public StyleValue<Position> Position
    {
        get => GetProperty<Position>(CommonProperties.Position);
        set => SetProperty(CommonProperties.Position, value);
    }

    public StyleValue<float> FlexGrow
    {
        get => GetProperty<float>(CommonProperties.FlexGrow);
        set => SetProperty(CommonProperties.FlexGrow, value);
    }

    public StyleValue<Justify> JustifyContent
    {
        get => GetProperty<Justify>(CommonProperties.JustifyContent);
        set => SetProperty(CommonProperties.JustifyContent, value);
    }

    public StyleValue<Align> AlignItems
    {
        get => GetProperty<Align>(CommonProperties.AlignItems);
        set => SetProperty(CommonProperties.AlignItems, value);
    }

    public StyleValue<bool> AlignSelfStretch
    {
        get => GetProperty<bool>(CommonProperties.AlignSelfStretch);
        set => SetProperty(CommonProperties.AlignSelfStretch, value);
    }

    public StyleValue<BorderOffsets> Margins
    {
        get => GetProperty<BorderOffsets>(CommonProperties.Margins);
        set => SetProperty(CommonProperties.Margins, value);
    }

    public StyleValue<BorderOffsets> Padding
    {
        get => GetProperty<BorderOffsets>(CommonProperties.Padding);
        set => SetProperty(CommonProperties.Padding, value);
    }

    public StyleValue<Vector2> AbsoluteOffset
    {
        get => GetProperty<Vector2>(CommonProperties.AbsoluteOffset);
        set => SetProperty(CommonProperties.AbsoluteOffset, value);
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
