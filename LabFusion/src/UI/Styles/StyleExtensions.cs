namespace LabFusion.UI.Styles;

public static class StyleExtensions
{
    public static T GetValueOrDefault<T>(this StyleValue<T> value, T defaultValue)
    {
        if (!value.HasValue())
        {
            return defaultValue;
        }

        return value;
    }

    public static bool HasValue(this StyleKeyword keyword) => keyword == StyleKeyword.Undefined;

    public static bool HasValue<T>(this StyleValue<T> value) => value.Keyword.HasValue();
}
