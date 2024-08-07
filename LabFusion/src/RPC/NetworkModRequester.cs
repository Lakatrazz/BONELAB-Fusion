using LabFusion.Downloading;
using LabFusion.Downloading.ModIO;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Client;

namespace LabFusion.RPC;

public static class NetworkModRequester
{
    public struct ModCallbackInfo
    {
        public ModIOFile modFile;
        public bool hasFile;
    }

    public struct ModRequestInfo
    {
        public byte target;

        public string barcode;

        public Action<ModCallbackInfo> modCallback;
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

    public static void RequestAndInstallMod(byte target, string barcode, DownloadCallback downloadCallback)
    {
        RequestMod(new ModRequestInfo()
        {
            target = target,
            barcode = barcode,
            modCallback = OnModInfoReceived,
        });

        void OnModInfoReceived(ModCallbackInfo info)
        {
            if (!info.hasFile)
            {
                return;
            }

            bool temporary = !ClientSettings.Downloading.KeepDownloadedMods.Value;

            ModIODownloader.EnqueueDownload(new ModTransaction()
            {
                modFile = info.modFile,
                temporary = temporary,
                callback = downloadCallback,
            });
        }
    }

    public static void RequestMod(ModRequestInfo info)
    {
        uint trackerId = _lastTrackedRequest++;

        if (info.modCallback != null)
        {
            _callbackQueue.Add(trackerId, info.modCallback);
        }

        // Send the request to the server
        using var writer = FusionWriter.Create(ModInfoRequestData.Size);
        var data = ModInfoRequestData.Create(PlayerIdManager.LocalSmallId, info.target, info.barcode, trackerId);
        writer.Write(data);

        using var request = FusionMessage.Create(NativeMessageTag.ModInfoRequest, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, request);
    }
}