using Il2CppSLZ.Marrow.Forklift.Model;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Downloading.ModIO;
using LabFusion.Marrow;
using LabFusion.Utilities;

using MelonLoader;

using System.Collections;
using System.IO.Compression;

using UnityEngine;

namespace LabFusion.Downloading;

public static class ModDownloadManager
{
    public static void LoadPalletFromZip(string path, ModIOFile modFile, bool cache, Action scheduledCallback = null, DownloadCallback downloadCallback = null)
    {
        MelonCoroutines.Start(CoLoadPalletFromZip(path, modFile, cache, scheduledCallback, downloadCallback));
    }

    private static IEnumerator CoLoadPalletFromZip(string path, ModIOFile modFile, bool cache, Action scheduledCallback = null, DownloadCallback downloadCallback = null)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var extractedDirectory = ModPathManager.ExportPath + "/" + fileName;

        // Delete the files if they already exist
        if (Directory.Exists(extractedDirectory))
        {
            Directory.Delete(extractedDirectory, true);
        }

        // Create new destination directory
        Directory.CreateDirectory(extractedDirectory);

        void UnzipMod()
        {
            using ZipArchive archive = ZipFile.OpenRead(path);

            archive.ExtractToDirectory(extractedDirectory, true);
        }

        var unzipTask = Task.Run(UnzipMod);

        while (!unzipTask.IsCompleted)
        {
            yield return null;
        }

        if (!unzipTask.IsCompletedSuccessfully)
        {
            FusionLogger.LogException($"unzipping mod at path {path}", unzipTask.Exception);

            downloadCallback?.Invoke(DownloadCallbackInfo.FailedCallback);

            scheduledCallback?.Invoke();

            yield break;
        }

#if DEBUG
        FusionLogger.Log($"Extracted pallet from {path} to {extractedDirectory}!");
#endif

        // Search for pallet path
        var extractedPallet = ModPathManager.FindPalletJson(extractedDirectory);

        if (string.IsNullOrWhiteSpace(extractedPallet))
        {
            FusionLogger.Warn($"Failed to find pallet json at {extractedDirectory}, aborting download!");

            downloadCallback?.Invoke(DownloadCallbackInfo.FailedCallback);

            scheduledCallback?.Invoke();

            yield break;
        }

        var palletDirectory = Path.GetDirectoryName(extractedPallet);

        if (string.IsNullOrWhiteSpace(palletDirectory))
        {
            FusionLogger.Warn($"Failed to get directory name of pallet {extractedPallet}, aborting download!");

            downloadCallback?.Invoke(DownloadCallbackInfo.FailedCallback);

            scheduledCallback?.Invoke();

            yield break;
        }

        var palletDirectoryInfo = new DirectoryInfo(palletDirectory);
        var palletDirectoryName = palletDirectoryInfo.Name;

        string modsDirectory = cache ? ModPathManager.CachePath : ModPathManager.ModsPath;

        string palletPath = modsDirectory + $"/{palletDirectoryName}";

        var existingPalletManifest = AssetWarehouseSearcher.GetManifest(new Barcode(palletDirectoryName));

        if (existingPalletManifest != null)
        {
            palletPath = Path.GetDirectoryName(existingPalletManifest.PalletPath);
        }

        // Delete pallet folder if it already exists
        if (Directory.Exists(palletPath))
        {
            Directory.Delete(palletPath, true);
        }

        // Move into mods folder
        Directory.Move(palletDirectory, palletPath);

        // Delete extracted folder
        Directory.Delete(extractedDirectory, true);

        // Add pallet to load queue
        var jsonPath = ModPathManager.FindPalletJson(palletPath);

#if DEBUG
        FusionLogger.Log($"Scheduling pallet for load at path {jsonPath}");
#endif

        StringModTargetListingDictionary targets = new();
        var modIoModTarget = new ModIOModTarget()
        {
            GameId = ModIOSettings.GameID,
            ModId = modFile.ModID,
            ModfileId = modFile.FileID.Value,
        };
        targets.Add(ModIOManager.GetActiveModTarget(), modIoModTarget);

        ModListing listing = new()
        {
            Author = null,
            Barcode = null,
            Description = null,
            Repository = null,
            Targets = targets,
        };

        var shipment = new ModForklift.PalletShipment()
        {
            palletPath = jsonPath,
            modListing = listing,
            callback = downloadCallback,
        };

        ModForklift.SchedulePalletLoad(shipment);

        // Run scheduled callback
        scheduledCallback?.Invoke();
    }

    internal static void Initialize()
    {
        ModPathManager.Initialize();
        ModCacheManager.Initialize();
    }
}
