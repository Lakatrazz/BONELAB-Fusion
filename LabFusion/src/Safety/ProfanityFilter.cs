using LabFusion.Preferences.Client;

namespace LabFusion.Safety;

/// <summary>
/// A simple profanity filter that blocks some of the more offensive words.
/// </summary>
public static class ProfanityFilter
{
    private const char _censorCharacter = '*';

    // This list contains offensive words, read at your own discretion
    // Words will be added as players use them in usernames, nicknames, descriptions, etc.
    // If you find any words that should be added to the list, create a pull request to submit them.
    private static readonly string[] _censoredWords = new string[]
    {
        "nigger",
        "nigga",
        "fag",
        "faggot",
        "dildo",
        "sex",
        "rape",
        "raping",
    };

    /// <summary>
    /// Returns text replaced by an asterisk if it contains profanity, or unedited otherwise.
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
            return new string(_censorCharacter, text.Length);
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

        foreach (var word in _censoredWords)
        {
            if (lower.Contains(word.ToLower()))
            {
                return true;
            }
        }

        return false;
    }
}
