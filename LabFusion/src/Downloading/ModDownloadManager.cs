using Il2CppSLZ.Marrow.Forklift.Model;

using LabFusion.Downloading.ModIO;
using LabFusion.Utilities;

using MelonLoader;

using System.Collections;
using System.IO.Compression;

using UnityEngine;

namespace LabFusion.Downloading;

public static class ModDownloadManager
{
    public static string ModsPath => Application.persistentDataPath + "/Mods";

    public static string ModsTempPath => Application.persistentDataPath + "/ModsTemp";

    public static string StagingPath => Application.persistentDataPath + "/FusionStaging";

    public static string DownloadPath => StagingPath + "/Downloads";

    public static string ExportPath => StagingPath + "/Exports";

    private const string _palletExtension = ".pallet.json";

    public static void DeleteTemporaryDirectories()
    {
        if (Directory.Exists(ModsTempPath))
        {
            Directory.Delete(ModsTempPath, true);
        }
    }

    public static void ValidateDirectories()
    {
        // Create the base staging directory
        if (!Directory.Exists(StagingPath))
        {
            Directory.CreateDirectory(StagingPath);
        }

        // Create the file path for temporarily loaded mods
        if (!Directory.Exists(ModsTempPath))
        {
            Directory.CreateDirectory(ModsTempPath);
        }

        // Create the file path for downloads
        if (!Directory.Exists(DownloadPath))
        {
            Directory.CreateDirectory(DownloadPath);
        }

        // Create the file path for extracted zips
        if (!Directory.Exists(ExportPath))
        {
            Directory.CreateDirectory(ExportPath);
        }
    }

    public static string FindPalletJson(string directory)
    {
        foreach (var file in Directory.GetFiles(directory))
        {
            if (file.EndsWith(_palletExtension))
            {
                return file;
            }
        }

        foreach (var subDirectory in Directory.GetDirectories(directory))
        {
            var file = FindPalletJson(subDirectory);

            if (!string.IsNullOrEmpty(file))
            {
                return file;
            }
        }

        return string.Empty;
    }

    public static void LoadPalletFromZip(string path, ModIOFile modFile, bool temporary, Action scheduledCallback = null, DownloadCallback downloadCallback = null)
    {
        MelonCoroutines.Start(CoLoadPalletFromZip(path, modFile, temporary, scheduledCallback, downloadCallback));
    }

    private static IEnumerator CoLoadPalletFromZip(string path, ModIOFile modFile, bool temporary, Action scheduledCallback = null, DownloadCallback downloadCallback = null)
    {
        using var routine = LoadPalletFromZipCore(path, modFile, temporary, downloadCallback);
        bool scheduled = false;

        try
        {
            while (true)
            {
                bool moved;
                try
                {
                    moved = routine.MoveNext();
                }
                catch (Exception e)
                {
                    FusionLogger.LogException("installing downloaded pallet", e);
                    break;
                }

                if (!moved)
                {
                    scheduled = true;
                    break;
                }

                yield return routine.Current;
            }
        }
        finally
        {
            try
            {
                var extractedDirectory = Path.Combine(ExportPath, Path.GetFileNameWithoutExtension(path));
                if (Directory.Exists(extractedDirectory))
                {
                    Directory.Delete(extractedDirectory, true);
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException("cleaning up extracted pallet", e);
            }

            try
            {
                if (!scheduled)
                {
                    downloadCallback?.Invoke(DownloadCallbackInfo.FailedCallback);
                }
            }
            finally
            {
                scheduledCallback?.Invoke();
            }
        }
    }

    private static IEnumerator<object> LoadPalletFromZipCore(string path, ModIOFile modFile, bool temporary, DownloadCallback downloadCallback)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var extractedDirectory = ExportPath + "/" + fileName;

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
            throw new IOException("Failed to extract the downloaded pallet.", unzipTask.Exception);
        }

#if DEBUG
        FusionLogger.Log($"Extracted pallet from {path} to {extractedDirectory}!");
#endif

        // Search for pallet path
        var extractedPallet = FindPalletJson(extractedDirectory);

        if (string.IsNullOrWhiteSpace(extractedPallet))
        {
            throw new InvalidDataException("The archive does not contain a pallet JSON file.");
        }

        var palletDirectory = Path.GetDirectoryName(extractedPallet);

        if (string.IsNullOrWhiteSpace(palletDirectory))
        {
            throw new InvalidDataException("The pallet directory is invalid.");
        }

        var palletDirectoryInfo = new DirectoryInfo(palletDirectory);

        var parentPath = ModsPath;

        if (temporary)
        {
            parentPath = ModsTempPath;
        }

        var palletPath = parentPath + $"/{palletDirectoryInfo.Name}";

        // Delete pallet folder if it already exists
        if (Directory.Exists(palletPath))
        {
            Directory.Delete(palletPath, true);
        }

        // Move into mods folder
        Directory.Move(palletDirectory, palletPath);

        // Delete extracted folder
        if (Directory.Exists(extractedDirectory))
        {
            Directory.Delete(extractedDirectory, true);
        }

        // Add pallet to load queue
        var jsonPath = FindPalletJson(palletPath);

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
        targets.Add(ModIOManager.GetActivePlatform(), modIoModTarget);

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
    }
}
