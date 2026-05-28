namespace LabFusion.UI.Styles;

public static class StyleExtensions
{
    public static T GetValueOrDefault<T>(this StyleValue<T> value, T defaultValue)
    {
        if (!value.Keyword.HasValue())
        {
            return defaultValue;
        }

        return value;
    }

    public static bool HasValue(this StyleKeyword keyword) => keyword == StyleKeyword.Undefined;
}
