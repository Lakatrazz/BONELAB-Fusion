using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow;

/// <summary>
/// References to assets in the AssetWarehouse that may be different across Marrow games.
/// </summary>
public static class MarrowGameReferences
{
    public static AvatarCrateReference CalibrationAvatarReference { get; set; } = new(MarrowBarcodes.EmptyBarcode);

    public static float CalibrationAvatarHeight { get; set; } = MarrowConstants.StandardHeight;
}
