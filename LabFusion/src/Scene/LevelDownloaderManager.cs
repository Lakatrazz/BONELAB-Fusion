using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Downloading;
using LabFusion.Downloading.ModIO;
using LabFusion.Marrow;
using LabFusion.Marrow.Proxies;
using LabFusion.Menu;
using LabFusion.Marrow.Patching;
using LabFusion.Preferences.Client;
using LabFusion.RPC;
using LabFusion.Utilities;

namespace LabFusion.Scene;

public static class LevelDownloaderManager
{
    public struct LevelDownloadInfo
    {
        public string LevelBarcode;
        public byte LevelHost;

        public Action OnDownloadSucceeded, OnDownloadFailed, OnDownloadCanceled;
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
        NetworkSceneManager.Purgatory = false;
    }

    public static void DownloadLevel(LevelDownloadInfo info)
    {
        _downloadingBarcode = info.LevelBarcode;
        _downloadingInfo = info;

        // Get the maximum amount of bytes that we download before cancelling, to make sure the level isn't too big
        long maxBytes = DataConversions.ConvertMegabytesToBytes(ClientSettings.Downloading.MaxLevelSize.Value);

        // Request the mod id from the host
        NetworkModRequester.RequestAndInstallMod(new NetworkModRequester.ModInstallInfo()
        {
            Target = info.LevelHost,
            Barcode = info.LevelBarcode,
            BeginDownloadCallback = OnDownloadBegin,
            FinishDownloadCallback = OnDownloadFinished,
            MaxBytes = maxBytes,
            HighPriority = true,
        });
    }

    private static void OnDownloadBegin(NetworkModRequester.ModCallbackInfo info)
    {
        _initializedDownloadUI = false;
        _downloadingLevel = true;
        _downloadingFile = info.ModFile;

        NetworkSceneManager.Purgatory = true;

        LoadWaitingScene();
    }

    private static void OnDownloadFinished(DownloadCallbackInfo info)
    {
        NetworkSceneManager.Purgatory = false;

        _downloadingLevel = false;
        _downloadingFile = new ModIOFile(-1);

        if (info.result == ModResult.CANCELED)
        {
            _downloadingInfo.OnDownloadCanceled?.Invoke();
            return;
        }

        if (info.result == ModResult.FAILED)
        {
            _downloadingInfo.OnDownloadFailed?.Invoke();
            return;
        }

        _downloadingInfo.OnDownloadSucceeded?.Invoke();
    }

    private static void LoadWaitingScene()
    {
        SceneStreamerPatches.IgnorePatches = true;

        SceneStreamer.Load(new Barcode(FusionLevelReferences.LoadDownloadingReference.Barcode));

        SceneStreamerPatches.IgnorePatches = false;
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

        if (_downloadingFile.ModID != -1)
        {
            ModIOThumbnailDownloader.GetThumbnail(_downloadingFile.ModID, (texture) =>
            {
                ui.LevelIcon.texture = texture;
            });
        }
    }
}
