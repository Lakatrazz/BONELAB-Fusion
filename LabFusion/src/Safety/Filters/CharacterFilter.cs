using System.Text.RegularExpressions;

namespace LabFusion.Safety;

/// <summary>
/// Filters for removing certain invalid character sets.
/// </summary>
public static class CharacterFilter
{
    public const string NonAlphanumericExpression = "[^a-zA-Z0-9]";

    public const string NonLatin1Expression = @"[^\u0000-\u00FF]";

    /// <summary>
    /// Removes all non-alphanumeric characters from a string.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string RemoveNonAlphanumeric(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        return Regex.Replace(text, NonAlphanumericExpression, string.Empty);
    }

    /// <summary>
    /// Replaces all characters in a string that aren't included in Latin-1 with <see cref="FilterSettings.InvalidCharacter"/>.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string FilterNonLatin1Characters(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        return Regex.Replace(text, NonLatin1Expression, FilterSettings.InvalidCharacter.ToString());
    }
}
