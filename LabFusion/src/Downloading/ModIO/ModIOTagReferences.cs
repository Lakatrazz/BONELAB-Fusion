namespace LabFusion.Downloading.ModIO;

public static class ModIOTagReferences
{
    #region MISC

    public static readonly string Replacer = "Replacer";

    #endregion

    #region ARRAYS

    public static readonly string[] BlacklistedTags = { Replacer, };

    #endregion

    /// <summary>
    /// Checks if a mod has any blacklisted tags and should not be downloaded.
    /// </summary>
    /// <param name="modData"></param>
    /// <returns></returns>
    public static bool HasBlacklistedTag(ModData modData)
    {
        foreach (var tag in BlacklistedTags)
        {
            if (modData.HasTag(tag))
            {
                return true;
            }
        }

        return false;
    }
}
