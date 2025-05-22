using LabFusion.Data;

using System.Text.Json.Serialization;

using UnityEngine;

namespace LabFusion.Safety;

[Serializable]
public class GlobalBanInfo
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = null;

    [JsonPropertyName("games")]
    public List<GameInfo> Games { get; set; } = new();

    [JsonPropertyName("platforms")]
    public List<PlatformInfo> Platforms { get; set; } = new();
}

[Serializable]
public class GlobalBanList
{
    [JsonPropertyName("bans")]
    public List<GlobalBanInfo> Bans { get; set; } = new();
}

public static class GlobalBanManager
{
    public const string FileName = "globalBans.json";

    public static GlobalBanList BanList { get; private set; } = new();

    public static void FetchFile()
    {
        ListFetcher.FetchFile(FileName, OnFileFetched);
    }

    private static void OnFileFetched(string text)
    {
        BanList = DataSaver.ReadJsonFromText<GlobalBanList>(text);
    }

    public static void ExportFile()
    {
        DataSaver.WriteJsonToFile(FileName, BanList);
    }

    public static void ExportBan(PlayerInfo playerInfo, string reason)
    {
        var game = new GameInfo() { Game = Application.productName };
        var games = new List<GameInfo>
        {
            game,
        };

        var platform = new PlatformInfo(playerInfo.LongId);
        var platforms = new List<PlatformInfo>
        {
            platform
        };

        var globalBanInfo = new GlobalBanInfo()
        {
            Username = playerInfo.Username,
            Reason = reason,
            Games = games,
            Platforms = platforms,
        };

        BanList.Bans.RemoveAll((info) => info.Platforms.Contains(platform));

        BanList.Bans.Add(globalBanInfo);

        ExportFile();
    }

    public static void ExportPardon(PlatformInfo platform)
    {
        BanList.Bans.RemoveAll((info) => info.Platforms.Contains(platform));

        ExportFile();
    }
}
