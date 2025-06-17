namespace LabFusion.SDK.Metadata;

public static class KeyHelper
{
    public const string PropertySeparator = ":";

    /// <summary>
    /// Checks if a metadata key matches a variable name.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="variable"></param>
    /// <returns></returns>
    public static bool KeyMatchesVariable(string key, string variable)
    {
        var lastSeparator = key.LastIndexOf(PropertySeparator);

        if (lastSeparator < 0)
        {
            return key == variable;
        }

        var subString = key[..lastSeparator];

        return subString == variable;
    }

    /// <summary>
    /// Creates a metadata variable key with a string property appended to the end.
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    public static string GetKeyWithProperty(string variable, string property)
    {
        return $"{variable}{PropertySeparator}{property}";
    }

    /// <summary>
    /// Gets a string property appended to a metadata variable key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetPropertyFromKey(string key)
    {
        var propertyStartIndex = key.LastIndexOf(PropertySeparator) + 1;
        return key[propertyStartIndex..];
    }

    /// <summary>
    /// Gets a metadata key for a player SmallID.
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="smallID"></param>
    /// <returns></returns>
    public static string GetKeyFromPlayer(string variable, byte smallID)
    {
        return GetKeyWithProperty(variable, smallID.ToString());
    }

    /// <summary>
    /// Gets a player SmallID from a given metadata variable key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static byte GetPlayerFromKey(string key)
    {
        var smallIDProperty = GetPropertyFromKey(key);

        if (!byte.TryParse(smallIDProperty, out var smallID))
        {
            throw new FormatException($"Key {key} was not in the correct format. (Property: {smallIDProperty})");
        }

        return smallID;
    }
}