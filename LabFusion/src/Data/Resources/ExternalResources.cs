using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow;

namespace LabFusion.Data;

/// <summary>
/// Handles external resources that are bundled in the platform specific content pallet.
/// </summary>
public static class ExternalResources
{
    /// <summary>
    /// Returns true if the external resources are currently available and ready to be loaded.
    /// </summary>
    public static bool IsAvailable { get; private set; } = false;

    /// <summary>
    /// Attempts to initialize the external resources if they have not been already. This will only work if the content pallet is properly installed and the AssetWarehouse is ready.
    /// </summary>
    public static void Initialize()
    {
        if (IsAvailable)
        {
            return;
        }

        if (!AssetWarehouse.ready)
        {
            return;
        }

        var contentPalletResourcesPath = GetContentPalletResourcesPath();
        var persistentResourcesPath = GetPersistentResourcesPath();

        if (contentPalletResourcesPath == null || persistentResourcesPath == null)
        {
            return;
        }

        if (Directory.Exists(persistentResourcesPath))
        {
            Directory.Delete(persistentResourcesPath, true);
        }

        FileCopier.CopyDirectoryRecursive(contentPalletResourcesPath, persistentResourcesPath);

        IsAvailable = true;
    }

    /// <summary>
    /// Gets the path to the external resources folder, if it is available.
    /// </summary>
    /// <returns></returns>
    public static string GetResourcesPath() => IsAvailable ? GetPersistentResourcesPath() : null;

    /// <summary>
    /// Tries to get the path to the external resources folder and returns if it succeeded.
    /// </summary>
    /// <param name="resourcesPath"></param>
    /// <returns></returns>
    public static bool TryGetResourcesPath(out string resourcesPath)
    {
        resourcesPath = GetResourcesPath();
        return !string.IsNullOrEmpty(resourcesPath);
    }

    /// <summary>
    /// Gets the absolute path to an external resource given a relative path.
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    public static string GetAbsolutePath(string relativePath)
    {
        if (!TryGetResourcesPath(out var resourcesPath))
        {
            return null;
        }

        return Path.Combine(resourcesPath, relativePath);
    }

    /// <summary>
    /// Tries to get an absolute path to an external resource and returns if it succeeded.
    /// </summary>
    /// <param name="relativePath"></param>
    /// <param name="absolutePath"></param>
    /// <returns></returns>
    public static bool TryGetAbsolutePath(string relativePath, out string absolutePath)
    {
        absolutePath = GetAbsolutePath(relativePath);
        return !string.IsNullOrEmpty(absolutePath);
    }

    private static string GetContentPalletResourcesPath()
    {
        if (!FusionPalletReferences.TryGetContentPalletManifest(out var palletManifest))
        {
            return null;
        }

        var palletDirectory = Path.GetDirectoryName(palletManifest.PalletPath);
        var resourcesPath = Path.Combine(palletDirectory, ResourcePaths.ExternalResourcesRelativePath);

        return resourcesPath;
    }

    private static string GetPersistentResourcesPath() => PersistentData.GetPath(ResourcePaths.ExternalResourcesRelativePath);
}
