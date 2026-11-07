using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow;

/// <summary>
/// References to Fusion's built-in pallets.
/// </summary>
public static class FusionPalletReferences
{
    /// <summary>
    /// A status indicating the presence of a Fusion pallet.
    /// </summary>
    public enum PalletStatus
    {
        /// <summary>
        /// The pallet is not installed.
        /// </summary>
        Missing,

        /// <summary>
        /// The pallet is an older version than is needed.
        /// </summary>
        Outdated,

        /// <summary>
        /// The pallet is installed and meets the necessary minimum version.
        /// </summary>
        Found
    }

    /// <summary>
    /// A reference to the FusionContent pallet. This contains all of Fusion's required assets.
    /// </summary>
    public static readonly PalletReference FusionContentReference = new("Lakatrazz.FusionContent");

    /// <summary>
    /// A reference to the FusionCosmetics pallet. This contains optional addon cosmetics for Fusion.
    /// </summary>
    public static readonly PalletReference FusionCosmeticsReference = new("Lakatrazz.FusionCosmetics");

    /// <summary>
    /// The minimum required version of the FusionContent pallet. If the installed pallet is missing or has a older version than this, then Fusion will attempt to update it, or prevent the user from playing.
    /// </summary>
    public static readonly Version MinimumContentVersion = new(1, 4, 0);

    /// <summary>
    /// Returns true if the Pallet for the FusionContent pallet was successfully found.
    /// </summary>
    /// <param name="pallet"></param>
    /// <returns></returns>
    public static bool TryGetContentPallet(out Pallet pallet)
    {
        pallet = AssetWarehouseSearcher.GetPallet(FusionContentReference.Barcode);
        return pallet != null;
    }

    /// <summary>
    /// Returns true if the PalletManifest for the FusionContent pallet was successfully found.
    /// </summary>
    /// <param name="palletManifest"></param>
    /// <returns></returns>
    public static bool TryGetContentPalletManifest(out PalletManifest palletManifest)
    {
        palletManifest = AssetWarehouseSearcher.GetManifest(FusionContentReference.Barcode);
        return palletManifest != null;
    }

    /// <summary>
    /// Returns the status of an installed pallet by reference.
    /// </summary>
    /// <param name="palletReference"></param>
    /// <param name="minimumVersion"></param>
    /// <returns></returns>
    public static PalletStatus ValidatePallet(PalletReference palletReference, Version minimumVersion)
    {
        var pallet = AssetWarehouseSearcher.GetPallet(palletReference.Barcode);

        if (pallet == null)
        {
            return PalletStatus.Missing;
        }

        var version = new Version(pallet.Version);

        if (version < minimumVersion)
        {
            return PalletStatus.Outdated;
        }

        return PalletStatus.Found;
    }

    /// <summary>
    /// Returns the status of the FusionContent pallet.
    /// </summary>
    /// <returns></returns>
    public static PalletStatus ValidateContentPallet()
    {
        return ValidatePallet(FusionContentReference, MinimumContentVersion);
    }
}