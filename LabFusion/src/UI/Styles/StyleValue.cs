namespace LabFusion.UI.Styles;

public interface IStyleValue
{
    object Value { get; set; }

    StyleKeyword Keyword { get; set; }

    IStyleValue Clone();
}

public struct StyleValue<T> : IStyleValue
{
    object IStyleValue.Value { readonly get => Value; set => Value = (T)value; }

    private T _value;
    public T Value
    {
        readonly get => _value;
        set
        {
            _value = value;
            _keyword = StyleKeyword.Undefined;
        }
    }

    private StyleKeyword _keyword;
    public StyleKeyword Keyword { readonly get => _keyword; set => _keyword = value; }

    public StyleValue(T value)
    {
        _value = value;
        _keyword = StyleKeyword.Undefined;
    }

    public StyleValue(StyleKeyword keyword)
    {
        _keyword = keyword;
        _value = default;
    }

    public IStyleValue Clone()
    {
        return new StyleValue<T>()
        {
            _value = _value,
            _keyword = _keyword,
        };
    }

    public static implicit operator T(StyleValue<T> value) => value.Value;

    public static implicit operator StyleValue<T>(T value) => new(value);

    public static implicit operator StyleKeyword(StyleValue<T> value) => value.Keyword;

    public static implicit operator StyleValue<T>(StyleKeyword keyword) => new(keyword);
}
