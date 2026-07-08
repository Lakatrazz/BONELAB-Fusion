using Il2CppSLZ.Marrow.Forklift.Model;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Utilities;
using LabFusion.UI.Popups;

namespace LabFusion.Downloading;

public static class ModForklift
{
    public struct PalletShipment
    {
        public string palletPath;
        public ModListing modListing;
        public DownloadCallback callback;

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

    public static void SchedulePalletLoad(PalletShipment shipment)
    {
        if (_scheduledShipments.Contains(shipment))
        {
            return;
        }

        _scheduledShipments.Enqueue(shipment);
    }

    private static void LoadPallet(PalletShipment shipment)
    {
        var palletPath = shipment.palletPath;

#if DEBUG
        FusionLogger.Log($"Loading pallet at path {palletPath}.");
#endif

        var warehouse = AssetWarehouse.Instance;
        var palletTask = warehouse.LoadPalletFromFolderAsync(palletPath, true, null, shipment.modListing);

        var onCompleted = () =>
        {
            // Get pallet from path
            Pallet foundPallet = null;
            PalletManifest foundManifest = null;

            var manifests = AssetWarehouse.Instance.GetPalletManifests();

            foreach (var manifest in manifests)
            {
                if (manifest.PalletPath == palletPath)
                {
                    foundPallet = manifest.Pallet;
                    foundManifest = manifest;
                    break;
                }
            }

            if (foundPallet != null)
            {
                UpdateModListing(foundPallet, foundManifest);

                DownloadNotifications.SendDownloadNotification(foundPallet.Title);
            }

            // Invoke complete callback
            var info = new DownloadCallbackInfo()
            {
                Pallet = foundPallet,
                Result = ModResult.SUCCEEDED,
            };

            shipment.callback?.Invoke(info);
        };
        palletTask.GetAwaiter().OnCompleted(onCompleted);
    }

    private static void UpdateModListing(Pallet pallet, PalletManifest palletManifest)
    {
        var modListing = palletManifest.ModListing;

        if (modListing == null)
        {
            return;
        }

        modListing.Barcode = pallet.Barcode;
        modListing.Title = pallet.Title;
        modListing.Description = pallet.Description;
        modListing.Author = pallet.Author;
        modListing.Version = pallet.Version;

        AssetWarehouse.Instance.UpdatePalletManifest(palletManifest);
    }
}
