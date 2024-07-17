using BoneLib.BoneMenu;

using LabFusion.Downloading.ModIO;

using UnityEngine;

namespace LabFusion.BoneMenu;

public static partial class BoneMenuCreator
{
    private static FunctionElement _fusionContentButton = null;
    private static FunctionElement _fusionCosmeticsButton = null;

    public static void CreateDownloadingMenu(Page page)
    {
        var downloadingCategory = page.CreatePage("Downloading", Color.cyan);
        
        _fusionContentButton = downloadingCategory.CreateFunction("Download Fusion Content", Color.white, InstallContent);
        _fusionCosmeticsButton = downloadingCategory.CreateFunction("Download Fusion Cosmetics", Color.white, InstallCosmetics);
    }

    private static void InstallContent()
    {
        _fusionContentButton.ElementName = "Installing Fusion Content";

        ModTransaction transaction = new()
        {
            modFile = new(ModReferences.FusionContentId),
            callback = (info) =>
            {
                _fusionContentButton.ElementName = "Download Fusion Content";
            }
        };

        ModIODownloader.EnqueueDownload(transaction);
    }

    private static void InstallCosmetics()
    {
        _fusionCosmeticsButton.ElementName = "Installing Fusion Cosmetics";

        ModTransaction transaction = new()
        {
            modFile = new(ModReferences.FusionCosmeticsId),
            callback = (info) =>
            {
                _fusionCosmeticsButton.ElementName = "Download Fusion Cosmetics";
            }
        };

        ModIODownloader.EnqueueDownload(transaction);
    }
}
