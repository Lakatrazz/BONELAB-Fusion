using MelonLoader;

using UnityEngine;

using System.Collections;

namespace LabFusion.Downloading.ModIO;

public static class ModIOThumbnailDownloader
{
    public static void GetThumbnail(int modId, Action<Texture> callback)
    {
        ModIOManager.GetMod(modId, OnModReceived);

        void OnModReceived(ModCallbackInfo info)
        {
            if (info.result == ModResult.FAILED)
            {
                return;
            }

            var url = info.data.ThumbnailUrl;

            GetThumbnail(url, callback);
        }
    }

    public static void GetThumbnail(string url, Action<Texture> callback)
    {
        MelonCoroutines.Start(CoDownloadThumbnail(url, callback));
    }

    private static IEnumerator CoDownloadThumbnail(string url, Action<Texture> callback)
    {
        var handler = new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Automatic,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        using HttpClient client = new(handler);

        var responseTask = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        while (!responseTask.IsCompleted)
        {
            yield return null;
        }

        var content = responseTask.Result.Content;

        var bytesTask = content.ReadAsByteArrayAsync();

        while (!bytesTask.IsCompleted)
        {
            yield return null;
        }

        var bytes = bytesTask.Result;

        var texture = new Texture2D(1, 1);

        ImageConversion.LoadImage(texture, bytes);

        callback?.Invoke(texture);
    }
}
