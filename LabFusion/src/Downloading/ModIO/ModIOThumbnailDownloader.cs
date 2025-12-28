using MelonLoader;

using UnityEngine;

using System.Collections;

using LabFusion.Preferences;

namespace LabFusion.Downloading.ModIO;

public static class ModIOThumbnailDownloader
{
    public static Dictionary<int, Texture> ThumbnailCache { get; } = new();

    public static void ClearCache()
    {
        foreach (var texture in ThumbnailCache.Values)
        {
            if (texture == null)
            {
                continue;
            }

            UnityEngine.Object.Destroy(texture);
        }

        ThumbnailCache.Clear();
    }

    public static void GetThumbnail(int modID, Action<Texture> callback)
    {
        if (ThumbnailCache.TryGetValue(modID, out var cachedTexture))
        {
            callback?.Invoke(cachedTexture);
            return;
        }

        callback += (texture) =>
        {
            ThumbnailCache[modID] = texture;
        };

        ModIOManager.GetMod(modID, OnModReceived);

        void OnModReceived(ModCallbackInfo info)
        {
            if (info.Result != ModResult.SUCCEEDED)
            {
                return;
            }

            // Check maturity
            if (info.Data.Mature && !CommonPreferences.ShowMatureMods)
            {
                return;
            }

            var url = info.Data.ThumbnailUrl;

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
