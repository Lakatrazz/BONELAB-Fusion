using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.XML;

using System.Text.Json.Serialization;

using System.Xml.Linq;

namespace LabFusion.SDK.Achievements;

public static class AchievementSaveManager
{
    [Serializable]
    public class AchievementPointer : IXMLPackable
    {
        [JsonPropertyName("data")]
        public string Data { get; set; }

        public AchievementPointer(Achievement achievement)
        {
            XElement entry = new(nameof(Data));
            achievement.Pack(entry);
            Data = entry.ToString();
        }

        public AchievementPointer() { }

        public void Pack(XElement element)
        {
            element.SetAttributeValue(nameof(Data), Data);
        }

        public void Unpack(XElement element)
        {
            element.TryGetAttribute(nameof(Data), out var unpackedData);
            Data = unpackedData;
        }
    }

    [Serializable]
    public class AchievementSaveData
    {
        [JsonPropertyName("pointers")]
        public Dictionary<string, AchievementPointer> Pointers { get; set; }

        public static AchievementSaveData CreateCurrent()
        {
            var data = new AchievementSaveData()
            {
                Pointers = _achievementPointers,
            };
            return data;
        }
    }

    private const string _filePath = "achievements.dat";
    private const string _backupPath = "achievements.dat.bak";

    public static void OnInitializeMelon()
    {
        Achievement.OnAchievementUpdated += SaveAchievement;
    }

    public static void WriteToFile()
    {
        DataSaver.WriteJsonToFile(_filePath, AchievementSaveData.CreateCurrent());
    }

    public static void WriteBackup()
    {
        string filePath = PersistentData.GetPath(_filePath);
        string backupPath = PersistentData.GetPath(_backupPath);

        if (File.Exists(filePath))
            File.Copy(filePath, backupPath, true);
    }

    public static void ReadFile()
    {
        var data = DataSaver.ReadJsonFromFile<AchievementSaveData>(_filePath);

        if (data == null)
        {
            return;
        }

        if (data.Pointers != null)
        {
            _achievementPointers = data.Pointers;
        }
    }

    public static void SaveAchievement(Achievement achievement)
    {
        if (!_achievementPointers.ContainsKey(achievement.Barcode))
            _achievementPointers.Add(achievement.Barcode, null);

        _achievementPointers[achievement.Barcode] = new AchievementPointer(achievement);

        WriteToFile();
    }

    public static Dictionary<string, AchievementPointer> Pointers => _achievementPointers;
    private static Dictionary<string, AchievementPointer> _achievementPointers = new();
}