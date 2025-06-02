using System.Text;
using System.Text.RegularExpressions;

namespace LabFusion.Extensions;

public static class StringExtensions
{
    public const char UniqueSeparator = '¬';

    public static string Contract(this IList<string> list)
    {
        StringBuilder builder = new();
        for (var i = 0; i < list.Count; i++)
        {
            var value = list[i];
            builder.Append(value);

            if (i + 1 < list.Count)
                builder.Append(UniqueSeparator);
        }

        return builder.ToString();
    }

    public static string[] Expand(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return Array.Empty<string>();

        return value.Split(UniqueSeparator);
    }

    public static string RemoveRichText(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        Regex rich = new(@"<[^>]*>");
        string plainText = str;

        if (rich.IsMatch(plainText))
        {
            plainText = rich.Replace(plainText, string.Empty);
        }

        return plainText;
    }

    public static string RemoveRichTextExceptColor(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        Regex rich = new(@"<(?!\W*(?i)color(?-i)\W*)[^>]*>");
        string plainText = str;

        if (rich.IsMatch(plainText))
        {
            plainText = rich.Replace(plainText, string.Empty);
        }

        return plainText;
    }

    public static string LimitLength(this string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        int plainLength = RemoveRichText(str).Length;

        if (plainLength <= maxLength)
        {
            return str;
        }

        int offset = str.Length - plainLength;

        return str.Substring(0, maxLength + offset);
    }

    // .NET Core's string hashing isn't deterministic on game restart, only during the same instance.
    // Credits to https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/#a-deterministic-gethashcode-implementation
    // In this case, I reaally need it to be deterministic, so
    public static int GetDeterministicHashCode(this string str)
    {
        unchecked
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}