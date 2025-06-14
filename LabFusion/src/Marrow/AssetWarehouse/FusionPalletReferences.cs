using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow;

public static class FusionPalletReferences
{
    public enum PalletStatus
    {
        MISSING,
        OUTDATED,
        FOUND
    }

    public static readonly PalletReference FusionContentReference = new("Lakatrazz.FusionContent");

    public static readonly PalletReference FusionCosmeticsReference = new("Lakatrazz.FusionCosmetics");

    public static readonly Version MinimumContentVersion = new(1, 3, 0);

    public static PalletStatus ValidatePallet(PalletReference palletReference, Version minimumVersion)
    {
        var warehouse = AssetWarehouse.Instance;

        var pallets = warehouse.GetPallets();

        foreach (var pallet in pallets)
        {
            if (pallet.Barcode != palletReference.Barcode)
            {
                continue;
            }

            var version = new Version(pallet.Version);

            if (version < minimumVersion)
            {
                return PalletStatus.OUTDATED;
            }

            return PalletStatus.FOUND;
        }

        return PalletStatus.MISSING;
    }

    public static PalletStatus ValidateContentPallet()
    {
        return ValidatePallet(FusionContentReference, MinimumContentVersion);
    }
}