namespace LabFusion.Data;

public static class ResourcePaths
{
    public const string RawChangelogPath = "LabFusion.dependencies.resources.text.raw_changelog.txt";
    public const string RawCreditsPath = "LabFusion.dependencies.resources.text.credits.raw_credits_0";

    /// <summary>
    /// The relative path for Fusion's persistent data inside of the main persistent data folder.
    /// </summary>
    public static readonly string PersistentDataRelativePath = "/Fusion";

    /// <summary>
    /// The relative path that stores extracted external resources both in the content pallet and the persistent data folder.
    /// </summary>
    public static readonly string ExternalResourcesRelativePath = Path.Combine("_Fusion", "FusionResources");
}
