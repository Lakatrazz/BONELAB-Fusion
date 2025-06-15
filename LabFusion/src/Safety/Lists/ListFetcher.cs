using System.Collections;

using MelonLoader;

using LabFusion.Utilities;

using System.Net;

namespace LabFusion.Safety;

public static class ListFetcher
{
    public const string LogPrefix = "ListFetcher -";

    public const string RepositoryURL = $"https://raw.githubusercontent.com/Lakatrazz/Fusion-Lists/{Branch}/";

    public const string Branch = "main";

    public static void FetchFile(string path, Action<string> callback)
    {
        var url = RepositoryURL + path;

        FusionLogger.Log($"{LogPrefix} Fetching {path} from url {url}...", ConsoleColor.DarkGreen);

        MelonCoroutines.Start(CoFetchFile(url, callback));
    }
    
    private static IEnumerator CoFetchFile(string url, Action<string> callback)
    {
        var handler = new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Automatic,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        using var client = new HttpClient(handler);

        var responseTask = client.GetAsync(url, HttpCompletionOption.ResponseContentRead);

        while (!responseTask.IsCompleted)
        {
            yield return null;
        }

        if (!responseTask.IsCompletedSuccessfully)
        {
            FusionLogger.LogException($"fetching list from {url}", responseTask.Exception);

            yield break;
        }

        var responseResult = responseTask.Result;

        if (responseResult.StatusCode != HttpStatusCode.OK)
        {
            FusionLogger.Warn($"Failed to fetch {url} with code: {responseResult.StatusCode}");
            yield break;
        }

        var content = responseResult.Content;

        var stringTask = content.ReadAsStringAsync();

        while (!stringTask.IsCompleted)
        {
            yield return null;
        }

        if (!stringTask.IsCompletedSuccessfully)
        {
            FusionLogger.LogException($"reading string content from {url}", stringTask.Exception);

            yield break;
        }

        FusionLogger.Log($"{LogPrefix} Finished fetching {url}!", ConsoleColor.DarkGreen);

        var result = stringTask.Result;

        callback?.Invoke(result);
    }
}
