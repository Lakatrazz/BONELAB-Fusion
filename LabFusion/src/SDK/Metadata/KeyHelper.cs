using LabFusion.Player;

namespace LabFusion.SDK.Metadata;

public static class KeyHelper
{
    public const string PropertySeparator = ":";

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

    public static string GetKeyWithProperty(string variable, string property)
    {
        return $"{variable}{PropertySeparator}{property}";
    }

    public static string GetPropertyFromKey(string key)
    {
        var propertyStartIndex = key.LastIndexOf(PropertySeparator) + 1;
        return key[propertyStartIndex..];
    }

    public static string GetKeyFromPlayer(string variable, PlayerID player)
    {
        if (player == null)
        {
            return string.Empty;
        }

        return GetKeyWithProperty(variable, player.PlatformID.ToString());
    }

    public static PlayerID GetPlayerFromKey(string key)
    {
        var idProperty = GetPropertyFromKey(key);

        if (!ulong.TryParse(idProperty, out var longId))
        {
            throw new FormatException($"Key {key} was not in the correct format. (Property: {idProperty})");
        }

        return PlayerIDManager.GetPlayerID(longId);
    }
}