using BoneLib.BoneMenu.Elements;

using LabFusion.Downloading.ModIO;

using UnityEngine;

namespace LabFusion.BoneMenu;

public static partial class BoneMenuCreator
{
    private static FunctionElement _fusionContentButton = null;
    private static FunctionElement _fusionCosmeticsButton = null;

    public static void CreateDownloadingMenu(MenuCategory category)
    {
        var downloadingCategory = category.CreateCategory("Downloading", Color.cyan);

        _fusionContentButton = downloadingCategory.CreateFunctionElement("Download Fusion Content", Color.white, InstallContent);
        _fusionCosmeticsButton = downloadingCategory.CreateFunctionElement("Download Fusion Cosmetics", Color.white, InstallCosmetics);
    }

    private static void InstallContent()
    {
        _fusionContentButton.SetName("Installing Fusion Content");

        ModTransaction transaction = new()
        {
            modFile = new(ModReferences.FusionContentId),
            callback = (info) =>
            {
                _fusionContentButton.SetName("Download Fusion Content");
            }
        };

        ModIODownloader.EnqueueDownload(transaction);
    }

    private static void InstallCosmetics()
    {
        _fusionContentButton.SetName("Installing Fusion Cosmetics");

        ModTransaction transaction = new()
        {
            modFile = new(ModReferences.FusionCosmeticsId),
            callback = (info) =>
            {
                _fusionContentButton.SetName("Download Fusion Cosmetics");
            }
        };

        ModIODownloader.EnqueueDownload(transaction);
    }
}
