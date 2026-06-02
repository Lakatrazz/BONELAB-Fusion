using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow;

/// <summary>
/// References to items in the Marrow backlot. 
/// Some references may also not be in the backlot but are provided by their game's respective module.
/// </summary>
public static class BacklotReferences
{
    public static SpawnableCrateReference ConstrainerReference { get; set; } = new(MarrowBarcodes.EmptyBarcode);
}
