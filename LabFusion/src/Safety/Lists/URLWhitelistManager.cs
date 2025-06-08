using LabFusion.Data;
using System.Text.Json.Serialization;

namespace LabFusion.Safety
{
    public class URLInfo
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    [Serializable]
    public class URLWhitelistList
    {
        [JsonPropertyName("whitelist")]
        public List<URLInfo> Whitelist { get; set; } = new();
        
    }

    public static class URLWhitelistManager
    {
        public const string FileName = "UrlWhitelist.json";

        public static URLWhitelistList urlList { get; private set; } = new();

        public static void FetchFile()
        {
            //Should be replaced with normal banlist repo
            ListFetcher.FetchAltUrlFile(FileName, "https://raw.githubusercontent.com/elijoeispog/URLBanningExample/main/", OnFileFetched);
        }

        private static void OnFileFetched(string text)
        {
            urlList = DataSaver.ReadJsonFromText<URLWhitelistList>(text);
        }

        public static bool IsLinkWhitelisted(string link, out string urlDomain)
        {
            Uri url = new Uri(link);

            urlDomain = url.Host;
            
            foreach(var whitelist in urlList.Whitelist)
            {
                if(urlDomain == whitelist.Url)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool isUrl(string value)
        {
            //checks text to see if its a valid url
            Uri uriResult;
            bool result = Uri.TryCreate(value, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return result;
        }
    }
}
