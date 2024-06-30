using BoneLib.BoneMenu.Elements;

using LabFusion.Downloading.ModIO;

using UnityEngine;

namespace LabFusion.BoneMenu;

public static partial class BoneMenuCreator
{
    public static void CreateDownloadingMenu(MenuCategory category)
    {
        var downloadingCategory = category.CreateCategory("Downloading", Color.cyan);

        downloadingCategory.CreateFunctionElement("Download Fusion Content", Color.white, InstallContent);
        downloadingCategory.CreateFunctionElement("Download Fusion Cosmetics", Color.white, InstallCosmetics);
    }

    private static void InstallContent()
    {
        ModTransaction transaction = new()
        {
            modFile = new(ModReferences.FusionContentId),
        };

        ModIODownloader.EnqueueDownload(transaction);
    }

    private static void InstallCosmetics()
    {
        ModTransaction transaction = new()
        {
            modFile = new(ModReferences.FusionCosmeticsId),
        };

        ModIODownloader.EnqueueDownload(transaction);
    }
}
