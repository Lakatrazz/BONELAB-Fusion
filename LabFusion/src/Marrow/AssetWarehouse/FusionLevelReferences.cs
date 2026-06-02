using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow;

/// <summary>
/// References to levels included in the Fusion Content pallet.
/// </summary>
public static class FusionLevelReferences
{
    /// <summary>
    /// The waiting scene while a mod level is downloading.
    /// </summary>
    public static readonly LevelCrateReference LoadDownloadingReference = new("Lakatrazz.FusionContent.Level.LoadDownloading");

    /// <summary>
    /// The scene included for testing Fusion SDK scripts.
    /// </summary>
    public static readonly LevelCrateReference FusionTestingReference = new("Lakatrazz.FusionContent.Level.FusionTesting");
}