using LabFusion.Marrow;

using LabFusion.SDK.Equippables;

namespace LabFusion.SDK.Wearables;

public static class WristWatchManager
{
    public static IWearableUIProvider PanelUI { get; set; } = null;

    public static string WristWatchBarcode => FusionSpawnableReferences.WristWatchReference.Barcode.ID;

    public static bool IsWristWatchEquipped() => EquippableManager.IsLocalEquipped(WristWatchBarcode);

    public static void EquipWristWatch(bool equip = true) => EquippableManager.EquipEquippable(WristWatchBarcode, equip);

    internal static void Initialize()
    {
        CreateWristWatch();
    }

    private static void CreateWristWatch()
    {
        var wristWatch = new WristWatchItem();

        EquippableManager.RegisterEquippable(wristWatch);
    }
}
