﻿using BoneLib.BoneMenu;

using LabFusion.Downloading.ModIO;
using LabFusion.Preferences.Client;

using UnityEngine;

namespace LabFusion.BoneMenu;

public static partial class BoneMenuCreator
{
    private static FunctionElement _fusionContentButton = null;
    private static FunctionElement _fusionCosmeticsButton = null;

    public static void CreateDownloadingMenu(Page page)
    {
        var downloadingPage = page.CreatePage("Downloading", Color.cyan);
        
        _fusionContentButton = downloadingPage.CreateFunction("Download Fusion Content", Color.white, InstallContent);
        _fusionCosmeticsButton = downloadingPage.CreateFunction("Download Fusion Cosmetics", Color.white, InstallCosmetics);

        CreateBoolPreference(downloadingPage, "Download Spawnables", ClientSettings.Downloading.DownloadSpawnables);
        CreateBoolPreference(downloadingPage, "Download Avatars", ClientSettings.Downloading.DownloadAvatars);
        CreateBoolPreference(downloadingPage, "Download Levels", ClientSettings.Downloading.DownloadLevels);

        CreateBoolPreference(downloadingPage, "Keep Downloaded Mods", ClientSettings.Downloading.KeepDownloadedMods);

        CreateIntPreference(downloadingPage, "Max File Size (MB)", 10, 0, 10000, ClientSettings.Downloading.MaxFileSize);
        CreateIntPreference(downloadingPage, "Max Level Size (MB)", 10, 0, 10000, ClientSettings.Downloading.MaxLevelSize);
    }

    private static void InstallContent()
    {
        _fusionContentButton.ElementName = "Installing Fusion Content";

        ModTransaction transaction = new()
        {
            ModFile = new(ModReferences.FusionContentId),
            Callback = (info) =>
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
            ModFile = new(ModReferences.FusionCosmeticsId),
            Callback = (info) =>
            {
                _fusionCosmeticsButton.ElementName = "Download Fusion Cosmetics";
            }
        };

        ModIODownloader.EnqueueDownload(transaction);
    }
}
