using Il2CppCysharp.Threading.Tasks;
using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Downloading;

public static class ModCacheManager
{
    public static void LoadModsFromCache()
    {
        var cachePath = ModPathManager.CachePath;

        if (!Directory.Exists(cachePath))
        {
            return;
        }

        var assetWarehouse = AssetWarehouse.Instance;

        assetWarehouse.LoadPalletsFromFolderAsync(cachePath).Forget();
    }

    public static void UnloadModsFromCache()
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

        foreach (var pallet in cachePallets)
        {
            assetWarehouse.UnloadPallet(pallet);
        }
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

    internal static void Initialize()
    {
        Action onReady = OnAssetWarehouseReady;
        AssetWarehouse.OnReady(onReady);
    }

    private static void OnAssetWarehouseReady()
    {
        LoadModsFromCache();
    }
}
