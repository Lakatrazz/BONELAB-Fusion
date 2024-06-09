using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.XML;
using System.Xml.Linq;

namespace LabFusion.SDK.Achievements
{
    public static class AchievementSaveManager
    {
        [Serializable]
        public class AchievementPointer : IXMLPackable
        {
            public string data;

            public AchievementPointer(Achievement achievement)
            {
                XElement entry = new(nameof(data));
                achievement.Pack(entry);
                data = entry.ToString();
            }

            public AchievementPointer() { }

            public void Pack(XElement element)
            {
                element.SetAttributeValue(nameof(data), data);
            }

            public void Unpack(XElement element)
            {
                element.TryGetAttribute(nameof(data), out data);
            }
        }

        [Serializable]
        public class AchievementSaveData
        {
            public Dictionary<string, AchievementPointer> pointers;

            public static AchievementSaveData CreateCurrent()
            {
                var data = new AchievementSaveData()
                {
                    pointers = _achievementPointers,
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
            DataSaver.WriteBinary(_filePath, AchievementSaveData.CreateCurrent());
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
            var data = DataSaver.ReadBinary<AchievementSaveData>(_filePath);

            if (data == null)
            {
                return;
            }

            if (data.pointers != null)
                _achievementPointers = data.pointers;
        }

        public static void SaveAchievement(Achievement achievement)
        {
            if (!_achievementPointers.ContainsKey(achievement.Barcode))
                _achievementPointers.Add(achievement.Barcode, null);

            _achievementPointers[achievement.Barcode] = new AchievementPointer(achievement);

            WriteToFile();
        }

        public static FusionDictionary<string, AchievementPointer> Pointers => _achievementPointers;
        private static FusionDictionary<string, AchievementPointer> _achievementPointers = new();
    }
}
