using Il2CppSLZ.Marrow.Forklift.Model;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Utilities;

namespace LabFusion.Downloading;

public static class ModForklift
{
    public struct PalletShipment
    {
        public string palletPath;
        public ModListing modListing;
        public Action onFinished;

        public override readonly int GetHashCode()
        {
            return palletPath.GetHashCode();
        }
    }

    private static readonly Queue<PalletShipment> _scheduledShipments = new();

    public static void UpdateForklift()
    {
        if (!AssetWarehouse.ready)
        {
            return;
        }

        if (_scheduledShipments.Count > 0)
        {
            LoadPallet(_scheduledShipments.Dequeue());
        }
    }

    private static void LoadPallet(PalletShipment shipment)
    {
#if DEBUG
        FusionLogger.Log($"Loading pallet at path {shipment.palletPath}.");
#endif

        var warehouse = AssetWarehouse.Instance;
        var palletTask = warehouse.LoadPalletFromFolderAsync(shipment.palletPath, true, null, shipment.modListing);

        var onCompleted = () =>
        {
            shipment.onFinished?.Invoke();
        };
        palletTask.GetAwaiter().OnCompleted(onCompleted);
    }

    public static void SchedulePalletLoad(PalletShipment shipment)
    {
        if (_scheduledShipments.Contains(shipment))
        {
            return;
        }

        _scheduledShipments.Enqueue(shipment);
    }
}
