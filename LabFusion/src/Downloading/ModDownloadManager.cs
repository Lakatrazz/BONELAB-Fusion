using Il2CppCysharp.Threading.Tasks;
using Il2CppSLZ.Marrow.Forklift.Model;

using LabFusion.Downloading.ModIO;
using LabFusion.Utilities;

using System.IO.Compression;

using UnityEngine;

namespace LabFusion.Downloading;

public static class ModDownloadManager
{
    public static string ModsPath => Application.persistentDataPath + "/Mods";

    public static string StagingPath => Application.persistentDataPath + "/FusionStaging";

    public static string DownloadPath => StagingPath + "/Downloads";

    public static string ExportPath => StagingPath + "/Exports";

    private const string _palletExtension = ".pallet.json";

    public static void ValidateStagingDirectory()
    {
        // Create the base staging directory
        if (!Directory.Exists(StagingPath))
        {
            Directory.CreateDirectory(StagingPath);
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

    public static async Task LoadPalletFromZip(string path, int modId, int modFileId, Action onFinished = null)
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
            archive.ExtractToDirectory(extractedDirectory);
        }

        await Task.Run(UnzipMod);

#if DEBUG
        FusionLogger.Log($"Extracted pallet from {path} to {extractedDirectory}!");
#endif

        // Search for pallet path
        var extractedPallet = FindPalletJson(extractedDirectory);
        var palletDirectory = Path.GetDirectoryName(extractedPallet);

        var palletDirectoryInfo = new DirectoryInfo(palletDirectory);

        var palletPath = ModsPath + $"/{palletDirectoryInfo.Name}";

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
        var jsonPath = FindPalletJson(palletPath);

#if DEBUG
        FusionLogger.Log($"Scheduling pallet for load at path {jsonPath}");
#endif

        StringModTargetListingDictionary targets = new();
        var modIoModTarget = new ModIOModTarget()
        {
            GameId = ModIOSettings.GameId,
            ModId = modId,
            ModfileId = modFileId,
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
            onFinished = onFinished,
        };

        ModForklift.SchedulePalletLoad(shipment);
    }
}
