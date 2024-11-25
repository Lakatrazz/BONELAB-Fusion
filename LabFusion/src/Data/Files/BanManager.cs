using System.Text.Json;
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
    private const string _fileName = "bans.json";

    public static BanList BanList { get; private set; } = new();

    public static void ReadFile()
    {
        BanList = new();

        var filePath = PersistentData.GetPath(_fileName);

        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);

            BanList = JsonSerializer.Deserialize<BanList>(json);
        }
    }

    private static void WriteFile()
    {
        var filePath = PersistentData.GetPath(_fileName);

        var json = JsonSerializer.Serialize(BanList);

        File.WriteAllText(filePath, json);
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