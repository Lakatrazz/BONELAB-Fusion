using LabFusion.Preferences.Client;

namespace LabFusion.Safety;

/// <summary>
/// A simple profanity filter that blocks some of the more offensive words.
/// </summary>
public static class ProfanityFilter
{
    /// <summary>
    /// Returns text replaced by <see cref="FilterSettings.CensorCharacter"/> if it contains profanity, or unedited otherwise.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <returns>The filtered text.</returns>
    public static string Filter(string text)
    {
        if (!ClientSettings.Safety.FilterProfanity.Value)
        {
            return text;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        if (ContainsProfanity(text))
        {
            return FilterSettings.CensorString(text.Length);
        }

        return text;
    }

    /// <summary>
    /// Checks if a string contains profanity from a list of censored words.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static bool ContainsProfanity(string text)
    {
        if (!ClientSettings.Safety.FilterProfanity.Value)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var lower = text.ToLower();

        foreach (var word in ProfanityListManager.List.Words)
        {
            if (lower.Contains(word.ToLower()))
            {
                return true;
            }
        }

        return false;
    }
}
