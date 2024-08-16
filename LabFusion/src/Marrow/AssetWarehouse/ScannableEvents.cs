using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Utilities;

namespace LabFusion.Marrow;

public static class ScannableEvents
{
    public static event Action OnAssetWarehouseReadyEvent;

    public static event Action<Barcode> OnPalletAddedEvent;

    public static void OnAssetWarehouseReady()
    {
        try
        {
            OnAssetWarehouseReadyEvent?.Invoke();
        }
        catch (Exception e)
        {
            FusionLogger.LogException("invoking OnAssetWarehouseReadyEvent", e);
        }

        var warehouse = AssetWarehouse.Instance;

        var onPalletAdded = OnPalletAdded;
        warehouse.OnPalletAdded += onPalletAdded;
    }

    private static void OnPalletAdded(Barcode barcode)
    {
        try
        {
            OnPalletAddedEvent?.Invoke(barcode);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("invoking OnPalletAddedEvent", e);
        }
    }
}
