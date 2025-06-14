using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow.Pool;

namespace LabFusion.Bonelab;

public static class BonelabSpawnableReferences
{
    public static readonly SpawnableCrateReference GachaCapsuleReference = new("SLZ.BONELAB.Content.Spawnable.UtilityGachaCapsule");

    public static readonly SpawnableCrateReference ConstrainerReference = new("c1534c5a-3813-49d6-a98c-f595436f6e73");

    public static readonly SpawnableCrateReference AmmoBoxLightReference = new("c1534c5a-683b-4c01-b378-6795416d6d6f");
    public static readonly SpawnableCrateReference AmmoBoxMediumReference = new("c1534c5a-57d4-4468-b5f0-c795416d6d6f");
    public static readonly SpawnableCrateReference AmmoBoxHeavyReference = new("c1534c5a-97a9-43f7-be30-6095416d6d6f");

    internal static void RegisterBlacklist()
    {
        SpawnableBlacklist.ClientSideBarcodes.Add(AmmoBoxLightReference.Barcode.ID);
        SpawnableBlacklist.ClientSideBarcodes.Add(AmmoBoxMediumReference.Barcode.ID);
        SpawnableBlacklist.ClientSideBarcodes.Add(AmmoBoxHeavyReference.Barcode.ID);
    }
}