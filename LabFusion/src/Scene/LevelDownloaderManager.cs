using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Downloading;
using LabFusion.Downloading.ModIO;
using LabFusion.Marrow;
using LabFusion.Marrow.Proxies;
using LabFusion.Menu;
using LabFusion.Patching;
using LabFusion.Preferences.Client;
using LabFusion.RPC;
using LabFusion.Utilities;

namespace LabFusion.Scene;

public static class LevelDownloaderManager
{
    public struct LevelDownloadInfo
    {
        public string levelBarcode;
        public byte levelHost;

        public Action onDownloadSucceeded, onDownloadFailed, onDownloadCanceled;
    }

    private static bool _initializedDownloadUI = false;
    private static bool _downloadingLevel = false;
    private static ModIOFile _downloadingFile = new(-1);
    private static string _downloadingBarcode = null;
    private static LevelDownloadInfo _downloadingInfo;

    public static void OnInitializeMelon()
    {
        MultiplayerHooking.OnUpdate += OnUpdate;

        MultiplayerHooking.OnDisconnected += OnDisconnect;
    }

    private static void OnDisconnect()
    {
        // Incase the player gets stuck in purgatory, disable it on disconnect
        CrossSceneManager.Purgatory = false;
    }

    public static void DownloadLevel(LevelDownloadInfo info)
    {
        _downloadingBarcode = info.levelBarcode;
        _downloadingInfo = info;

        // Get the maximum amount of bytes that we download before cancelling, to make sure the level isn't too big
        long maxBytes = DataConversions.ConvertMegabytesToBytes(ClientSettings.Downloading.MaxLevelSize.Value);

        // Request the mod id from the host
        NetworkModRequester.RequestAndInstallMod(new NetworkModRequester.ModInstallInfo()
        {
            target = info.levelHost,
            barcode = info.levelBarcode,
            beginDownloadCallback = OnDownloadBegin,
            finishDownloadCallback = OnDownloadFinished,
            maxBytes = maxBytes,
            highPriority = true,
        });
    }

    private static void OnDownloadBegin(NetworkModRequester.ModCallbackInfo info)
    {
        _initializedDownloadUI = false;
        _downloadingLevel = true;
        _downloadingFile = info.modFile;

        CrossSceneManager.Purgatory = true;

        LoadWaitingScene();
    }

    private static void OnDownloadFinished(DownloadCallbackInfo info)
    {
        CrossSceneManager.Purgatory = false;

        _downloadingLevel = false;
        _downloadingFile = new ModIOFile(-1);

        if (info.result == ModResult.CANCELED)
        {
            _downloadingInfo.onDownloadCanceled?.Invoke();
            return;
        }

        if (info.result == ModResult.FAILED)
        {
            _downloadingInfo.onDownloadFailed?.Invoke();
            return;
        }

        _downloadingInfo.onDownloadSucceeded?.Invoke();
    }

    private static void LoadWaitingScene()
    {
        SceneLoadPatch.IgnorePatches = true;

        SceneStreamer.Load(new Barcode(FusionLevelReferences.LoadDownloadingReference.Barcode));

        SceneLoadPatch.IgnorePatches = false;
    }

    private static void OnUpdate()
    {
        if (!_downloadingLevel || ModIODownloader.CurrentTransaction == null)
        {
            return;
        }

        float progress = ModIODownloader.CurrentTransaction.Progress;

        var ui = LevelDownloadUI.Instance;

        if (ui == null)
        {
            return;
        }

        if (!_initializedDownloadUI)
        {
            MenuButtonHelper.PopulateTexts(ui.gameObject);

            ui.LevelTitleText.text = $"DOWNLOADING {_downloadingBarcode}";

            SetUIIcon(ui);

            _initializedDownloadUI = true;
        }

        ui.ProgressBarSlider.value = progress;
        ui.ProgressBarText.text = $"{progress * 100f}%";

        if (progress >= 1f)
        {
            ui.LevelTitleText.text = "DOWNLOAD COMPLETE";
        }
    }

    private static void SetUIIcon(LevelDownloadUI ui) 
    {
        var levelIcon = MenuResources.GetLevelIcon(MenuResources.ModsIconTitle);

        ui.LevelIcon.texture = levelIcon;

        if (_downloadingFile.ModId != -1)
        {
            ModIOThumbnailDownloader.GetThumbnail(_downloadingFile.ModId, (texture) =>
            {
                ui.LevelIcon.texture = texture;
            });
        }
    }
}
