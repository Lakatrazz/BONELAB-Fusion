using LabFusion.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LabFusion.Safety
{
    public class URLBanInfo
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }

    [Serializable]
    public class URLBanList
    {
        [JsonPropertyName("bans")]
        public List<URLBanInfo> Bans { get; set; } = new();
        
    }

    public static class URLBanManager
    {
        public const string FileName = "UrlBans.json";

        public static URLBanList urlList { get; private set; } = new();

        public static void FetchFile()
        {
            //Should be replaced with normal banlist repo
            ListFetcher.FetchAltUrlFile(FileName, "https://raw.githubusercontent.com/elijoeispog/URLBanningExample/main/", OnFileFetched);
        }

        private static void OnFileFetched(string text)
        {
            urlList = DataSaver.ReadJsonFromText<URLBanList>(text);
        }

        public static bool IsLinkBanned(string link, out string reason)
        {
            Uri url = new Uri(link);

            String domain = url.Host;
            foreach(var ban in urlList.Bans)
            {
                if(domain == ban.Url)
                {
                    reason = ban.Reason;
                    return true;
                }
            }
            reason = null;
            return false;
        }
    }
}
