using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow;

namespace LabFusion.Data;

public static class ExternalResources
{
    /// <summary>
    /// Returns true if the external resources are currently available and ready to be loaded.
    /// </summary>
    public static bool IsAvailable => AssetWarehouse.ready;

    public static string GetResourcesPath()
    {
        if (!FusionPalletReferences.TryGetContentPalletManifest(out var palletManifest))
        {
            return null;
        }

        var palletDirectory = Path.GetDirectoryName(palletManifest.PalletPath);
        var resourcesPath = Path.Combine(palletDirectory, ResourcePaths.ExternalResourcesRelativePath);

        return resourcesPath;
    }

    public static bool TryGetResourcesPath(out string resourcesPath)
    {
        resourcesPath = GetResourcesPath();
        return !string.IsNullOrEmpty(resourcesPath);
    }

    public static string GetAbsolutePath(string relativePath)
    {
        if (!TryGetResourcesPath(out var resourcesPath))
        {
            return null;
        }

        return Path.Combine(resourcesPath, relativePath);
    }

    public static bool TryGetAbsolutePath(string relativePath, out string absolutePath)
    {
        absolutePath = GetAbsolutePath(relativePath);
        return !string.IsNullOrEmpty(absolutePath);
    }
}
