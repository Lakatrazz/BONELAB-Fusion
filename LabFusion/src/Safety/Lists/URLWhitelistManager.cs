using LabFusion.Data;

using System.Text.Json.Serialization;

namespace LabFusion.Safety;

[Serializable]
public class URLInfo
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; }
}

[Serializable]
public class URLWhitelist
{
    [JsonPropertyName("whitelist")]
    public List<URLInfo> Whitelist { get; set; } = new();
}

public static class URLWhitelistManager
{
    public const string FileName = "urlWhitelist.json";

    public static URLWhitelist List { get; private set; } = new();

    public static void FetchFile()
    {
        ListFetcher.FetchFile(FileName, OnFileFetched);
    }

    private static void OnFileFetched(string text)
    {
        List = DataSaver.ReadJsonFromText<URLWhitelist>(text);
    }

    public static bool IsURLWhitelisted(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return true;
        }

        bool isLink = uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;

        if (!isLink)
        {
            return true;
        }

        var domain = uri.Host;

        foreach(var whitelist in List.Whitelist)
        {
            if (domain == whitelist.Domain)
            {
                return true;
            }
        }

        return false;
    }
}
