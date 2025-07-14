using System.Collections;

using LabFusion.Downloading;
using LabFusion.Downloading.ModIO;
using LabFusion.Network;
using LabFusion.Preferences.Client;
using LabFusion.Utilities;

using MelonLoader;

namespace LabFusion.RPC;

public static class NetworkModRequester
{
    public struct ModCallbackInfo
    {
        public ModIOFile ModFile;
        public bool HasFile;
        public string Platform;
    }

    public struct ModRequestInfo
    {
        public byte Target;

        public string Barcode;

        public Action<ModCallbackInfo> ModCallback;
    }

    public struct ModInstallInfo
    {
        public byte Target;

        public string Barcode;

        public Action<ModCallbackInfo> BeginDownloadCallback;

        public DownloadCallback FinishDownloadCallback;

        public long? MaxBytes;

        public IProgress<float> Reporter;

        public bool HighPriority;
    }

    private static uint _lastTrackedRequest = 0;

    private static readonly Dictionary<uint, Action<ModCallbackInfo>> _callbackQueue = new();

    public static void OnResponseReceived(uint trackerId, ModCallbackInfo info)
    {
        if (_callbackQueue.TryGetValue(trackerId, out var callback))
        {
            callback(info);
            _callbackQueue.Remove(trackerId);
        }
    }

    public static void RequestAndInstallMod(ModInstallInfo installInfo)
    {
        MelonCoroutines.Start(WaitAndInstallMod(installInfo));
    }

    private static IEnumerator WaitAndInstallMod(ModInstallInfo installInfo)
    {
        float elapsed = 0f;
        bool receivedCallback = false;

        RequestMod(new ModRequestInfo()
        {
            Target = installInfo.Target,
            Barcode = installInfo.Barcode,
            ModCallback = OnModInfoReceived,
        });

        // Wait for timeout
        while (!receivedCallback && elapsed < 5f)
        {
            elapsed += TimeUtilities.DeltaTime;
            yield return null;
        }

        // No callback means this request timed out
        if (!receivedCallback)
        {
#if DEBUG
            FusionLogger.Warn($"Mod request for {installInfo.Barcode} timed out.");
#endif

            installInfo.FinishDownloadCallback?.Invoke(DownloadCallbackInfo.FailedCallback);

            // Remove the callbacks incase it gets received very late
            installInfo.BeginDownloadCallback = null;
            installInfo.FinishDownloadCallback = null;
        }

        void OnModInfoReceived(ModCallbackInfo info)
        {
            receivedCallback = true;

            if (!info.HasFile)
            {
#if DEBUG
                FusionLogger.Warn("Mod info did not have a file, cancelling download.");
#endif

                installInfo.FinishDownloadCallback?.Invoke(DownloadCallbackInfo.FailedCallback);
                return;
            }

            installInfo.BeginDownloadCallback?.Invoke(info);

            bool temporary = !ClientSettings.Downloading.KeepDownloadedMods.Value;

            // If high priority, cancel other downloads
            if (installInfo.HighPriority)
            {
                ModIODownloader.CancelQueue();
            }

            ModIODownloader.EnqueueDownload(new ModTransaction()
            {
                ModFile = info.ModFile,
                Temporary = temporary,
                Callback = installInfo.FinishDownloadCallback,
                MaxBytes = installInfo.MaxBytes,
                Reporter = installInfo.Reporter,
            });
        }
    }

    public static void RequestMod(ModRequestInfo info)
    {
        uint trackerId = _lastTrackedRequest++;

        if (info.ModCallback != null)
        {
            _callbackQueue.Add(trackerId, info.ModCallback);
        }

        // Send the request to the server
        var data = new ModInfoRequestData()
        {
            Barcode = info.Barcode,
            TrackerID = trackerId,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.ModInfoRequest, new MessageRoute(info.Target, NetworkChannel.Reliable));
    }
}