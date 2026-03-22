using LabFusion.Extensions;

using System.Text.RegularExpressions;

namespace LabFusion.Safety;

/// <summary>
/// Helper for applying multiple common safety filters to a string.
/// </summary>
public static class TextFilter
{
    public const string NonAlphanumericExpression = "[^a-zA-Z0-9]";

    /// <summary>
    /// Filters a text with multiple common safety filters.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <returns>The filtered text.</returns>
    public static string FilterCommon(string text)
    {
        return ProfanityFilter.Filter(text.RemoveRichTextExceptColor());
    }

    /// <summary>
    /// Removes all non-alphanumeric characters from a string to perform checks against.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string RemoveNonAlphanumeric(string text)
    {
        return Regex.Replace(text, "[^a-zA-Z0-9]", string.Empty);
    }

    /// <summary>
    /// Removes special characters and rich text from a string so that it can be used for name comparisons.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string SanitizeName(string name)
    {
        return RemoveNonAlphanumeric(name.RemoveRichText());
    }
}
