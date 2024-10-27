using System.Security.Cryptography;

namespace LabFusion.Network;

public static class RandomCodeGenerator
{
    public const string StringCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string GetString(int length)
    {
        return new string(Enumerable.Repeat(StringCharacters, length)
            .Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
    }
}