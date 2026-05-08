namespace LabFusion.Safety;

/// <summary>
/// Functions to apply filters to specific info fields, such as names or descriptions.
/// </summary>
public static class InfoFilter
{
    /// <summary>
    /// Filters an identifier provided by the platform.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public static string FilterIdentifier(string identifier)
    {
        return TextFilter.FilterCommonAndRichText(identifier);
    }

    /// <summary>
    /// Filters a display name provided by the user.
    /// </summary>
    /// <param name="displayName"></param>
    /// <returns></returns>
    public static string FilterDisplayName(string displayName)
    {
        return TextFilter.FilterCommon(displayName);
    }

    /// <summary>
    /// Filters a description provided by the user.
    /// </summary>
    /// <param name="description"></param>
    /// <returns></returns>
    public static string FilterDescription(string description)
    {
        return TextFilter.FilterCommon(description);
    }
}
