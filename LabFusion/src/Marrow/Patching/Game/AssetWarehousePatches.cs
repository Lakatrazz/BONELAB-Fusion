using HarmonyLib;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(AssetWarehouse))]
public static class AssetWarehousePatches
{
    /// <summary>
    /// This should be set when any mods that are held or cached in a folder separate from the game's mods folder have been loaded.
    /// By default this is false, and it prevents pallet manifests from being incorrectly cleared by the game when the extra mods haven't been loaded yet.
    /// This is necessary for mods downloaded from other players, as they are held in a cache separate from the normal mods.
    /// </summary>
    public static bool LoadedAdditionalMods
    {
        get => _loadedAdditionalMods;
        set
        {
            if (_loadedAdditionalMods == value)
            {
                return;
            }

            _loadedAdditionalMods = value;

            if (value)
            {
                var assetWarehouse = AssetWarehouse.Instance;

                if (assetWarehouse != null && assetWarehouse.initialized)
                {
                    assetWarehouse.CleanupPalletManifests();
                }
            }
        }
    }

    private static bool _loadedAdditionalMods = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(AssetWarehouse.CleanupPalletManifests))]
    private static bool CleanupPalletManifestsPrefix()
    {
        if (!LoadedAdditionalMods)
        {
            return false;
        }

        return true;
    }
}
