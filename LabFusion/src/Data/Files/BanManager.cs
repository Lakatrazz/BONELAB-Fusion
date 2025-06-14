using System.Text.Json.Serialization;

namespace LabFusion.Data;

[Serializable]
public class BanInfo
{
    [JsonPropertyName("player")]
    public PlayerInfo Player { get; set; } = null;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = null;
}

[Serializable]
public class BanList
{
    [JsonPropertyName("bans")]
    public List<BanInfo> Bans { get; set; } = new();
}

public static class BanManager
{
    public const string FileName = "bans.json";

    public static BanList BanList { get; private set; } = new();

    public static void ReadFile()
    {
        BanList = new();

        var deserializedList = DataSaver.ReadJsonFromFile<BanList>(FileName);

        if (deserializedList != null)
        {
            BanList = deserializedList;
        }
    }

    private static void WriteFile()
    {
        DataSaver.WriteJsonToFile(FileName, BanList);
    }

    public static void Ban(PlayerInfo playerInfo, string reason)
    {
        var banInfo = new BanInfo()
        {
            Player = playerInfo,
            Reason = reason,
        };

        BanList.Bans.RemoveAll((info) => info.Player.LongId == playerInfo.LongId);

        BanList.Bans.Add(banInfo);

        WriteFile();
    }

    public static void Pardon(ulong longId)
    {
        BanList.Bans.RemoveAll((info) => info.Player.LongId == longId);

        WriteFile();
    }
}