using System.Text.RegularExpressions;

namespace LabFusion.Safety;

/// <summary>
/// A simple filter that removes all links.
/// </summary>
public static class LinkFilter
{
    public const string LinkExpression = @"[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\\=]*)";

    public static string Filter(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        return Regex.Replace(text, LinkExpression, string.Empty);
    }
}
