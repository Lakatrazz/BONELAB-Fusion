using Il2CppCysharp.Threading.Tasks;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Marrow;
using LabFusion.Preferences.Client;
using LabFusion.Utilities;

namespace LabFusion.Downloading;

public static class ModCacheManager
{
    public static int MaxCacheGigabytes
    {
        get
        {
            if (PlatformHelper.IsAndroid)
            {
                return 20;
            }

            return ClientSettings.Downloading.MaxCacheSize.Value;
        }
    }

    public static long MaxCacheBytes => DataConversions.ConvertGigabytesToBytes(MaxCacheGigabytes);

    public struct CachePalletInfo
    {
        public string Barcode;

        public DateTime LastUseTime;

        public string DirectoryPath;

        public long DirectorySize;
    }

    public static void LoadModsFromCache()
    {
        var cachePath = ModPathManager.CachePath;

        if (!Directory.Exists(cachePath))
        {
            return;
        }

        var assetWarehouse = AssetWarehouse.Instance;

        try
        {
            assetWarehouse.LoadPalletsFromFolderAsync(cachePath).Forget();
        }
        catch (Exception e)
        {
            FusionLogger.LogException("loading mods from cache", e);
        }
    }

    public static void UnloadModsFromCache()
    {
        var assetWarehouse = AssetWarehouse.Instance;

        List<Pallet> cachePallets = GetCachePallets();

        foreach (var pallet in cachePallets)
        {
            assetWarehouse.UnloadPallet(pallet);
        }
    }

    public static void DeleteAndUnloadPallet(string directoryPath, string barcode)
    {
        var assetWarehouse = AssetWarehouse.Instance;

        assetWarehouse.UnloadPallet(new Barcode(barcode));

        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }
    }

    public static List<CachePalletInfo> GetCachePalletInfos()
    {
        var cachePath = Path.GetFullPath(ModPathManager.CachePath);

        List<CachePalletInfo> cachePallets = new();

        if (!Directory.Exists(cachePath))
        {
            return cachePallets;
        }

        try
        {
            foreach (var directoryPath in Directory.EnumerateDirectories(cachePath))
            {
                var directoryInfo = new DirectoryInfo(directoryPath);

                var barcode = directoryInfo.Name;
                var lastUseTime = PalletUseHistoryManager.GetLastUseTime(barcode);
                var directorySize = FileSizeHelper.GetDirectorySize(directoryPath);

                var cachePalletInfo = new CachePalletInfo()
                {
                    Barcode = barcode,
                    LastUseTime = lastUseTime,
                    DirectoryPath = directoryPath,
                    DirectorySize = directorySize,
                };

                cachePallets.Add(cachePalletInfo);
            }
        }
        catch (UnauthorizedAccessException) { }

        return cachePallets;
    }

    public static List<Pallet> GetCachePallets()
    {
        var assetWarehouse = AssetWarehouse.Instance;

        var palletManifests = assetWarehouse.GetPalletManifests();

        var cachePath = Path.GetFullPath(ModPathManager.CachePath);

        List<Pallet> cachePallets = new();

        foreach (var manifest in palletManifests)
        {
            var palletPath = manifest.PalletPath;

            if (string.IsNullOrWhiteSpace(palletPath))
            {
                continue;
            }

            var palletDirectory = Path.GetDirectoryName(palletPath);
            var topDirectory = Path.GetDirectoryName(palletDirectory);

            var topDirectoryPath = Path.GetFullPath(topDirectory);

            bool samePath = Path.GetRelativePath(topDirectoryPath, cachePath) == ".";

            if (samePath)
            {
                cachePallets.Add(manifest.Pallet);
            }
        }

        return cachePallets;
    }

    public static bool FreeCache() => FreeCache(0);

    public static bool FreeCache(long minimumBytes)
    {
        var usedSpace = GetUsedSpace();
        var maxSpace = MaxCacheBytes;

        var freeSpace = maxSpace - usedSpace;

        if (freeSpace >= minimumBytes)
        {
            return true;
        }

        var neededSpace = minimumBytes - freeSpace;

        var oldestPallets = GetCachePalletInfos()
            .Where(i => !PalletUseHistoryManager.IsPalletActivelyUsed(i.Barcode))
            .OrderBy(i => i.LastUseTime);

        long removedSpace = 0;
        int lastIndex = -1;

        for (var i = 0; i < oldestPallets.Count(); i++)
        {
            lastIndex = i;

            var palletInfo = oldestPallets.ElementAt(i);

            removedSpace += palletInfo.DirectorySize;

            if (removedSpace >= neededSpace)
            {
                break;
            }
        }

        if (removedSpace < neededSpace)
        {
            return false;
        }

        for (var i = 0; i <= lastIndex; i++)
        {
            var palletInfo = oldestPallets.ElementAt(i);

            DeleteAndUnloadPallet(palletInfo.DirectoryPath, palletInfo.Barcode);
        }

        return true;
    }

    public static void ClearCache()
    {
        UnloadModsFromCache();

        var cachePath = ModPathManager.CachePath;

        if (Directory.Exists(cachePath))
        {
            Directory.Delete(cachePath, true);
        }
    }

    public static long GetUsedSpace()
    {
        var cachePath = Path.GetFullPath(ModPathManager.CachePath);

        return FileSizeHelper.GetDirectorySize(cachePath);
    }

    internal static void Initialize()
    {
        FreeCache();

        Action onReady = OnAssetWarehouseReady;
        AssetWarehouse.OnReady(onReady);
    }

    private static void OnAssetWarehouseReady()
    {
        LoadModsFromCache();
    }
}
