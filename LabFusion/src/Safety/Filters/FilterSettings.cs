namespace LabFusion.Safety;

public static class FilterSettings
{
    /// <summary>
    /// The character that filtered text gets replaced with.
    /// </summary>
    public const char CensorCharacter = '*';

    /// <summary>
    /// The character that invalid characters get replaced with.
    /// </summary>
    public const char InvalidCharacter = '?';

    /// <summary>
    /// Creates a string of repeating <see cref="CensorCharacter"/> with a specified length.
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string CensorString(int length) => new(CensorCharacter, length);
}
