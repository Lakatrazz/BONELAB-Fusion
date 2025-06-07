using LabFusion.Extensions;

namespace LabFusion.Safety;

/// <summary>
/// Helper for applying multiple common safety filters to a string.
/// </summary>
public static class TextFilter
{
    /// <summary>
    /// Filters a text with multiple common safety filters.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <returns>The filtered text.</returns>
    public static string Filter(string text)
    {
        return ProfanityFilter.Filter(text.RemoveRichTextExceptColor());
    }
}
