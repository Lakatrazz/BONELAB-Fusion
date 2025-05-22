using LabFusion.Data;
using LabFusion.Debugging;

using System.Text.Json.Serialization;

namespace LabFusion.SDK.Points;

using System;

public static class PointSaveManager
{
    [Serializable]
    public class PointSaveData
    {
        [JsonPropertyName("boughtItems")]
        public string[] BoughtItems { get; set; }

        [JsonPropertyName("enabledItems")]
        public string[] EnabledItems { get; set; }

        [JsonPropertyName("upgradedItems")]
        public Dictionary<string, int> UpgradedItems { get; set; }

        [JsonPropertyName("bitCount")]
        public int BitCount { get; set; }

        public static PointSaveData CreateCurrent()
        {
            var data = new PointSaveData
            {
                BoughtItems = _unlockedItems.ToArray(),
                EnabledItems = _equippedItems.ToArray(),
                UpgradedItems = _itemUpgrades,
                BitCount = _totalBits,
            };
            return data;
        }
    }

    private const string _filePath = "point_shop.dat";
    private const string _backupPath = "point_shop.dat.bak";

    public static void WriteToFile()
    {
        DataSaver.WriteJsonToFile(_filePath, PointSaveData.CreateCurrent());
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
        var data = DataSaver.ReadJsonFromFile<PointSaveData>(_filePath);

        if (data == null)
        {
            return;
        }

        if (data.BoughtItems != null)
            _unlockedItems = data.BoughtItems.ToList();

        if (data.EnabledItems != null)
            _equippedItems = data.EnabledItems.ToList();

        if (data.UpgradedItems != null)
            _itemUpgrades = data.UpgradedItems;

        _totalBits = data.BitCount;
    }

    public static int GetUpgradeLevel(string barcode)
    {
        if (_itemUpgrades.TryGetValue(barcode, out var level))
            return level;
        else
            return -1;
    }

    public static bool IsUnlocked(string barcode)
    {
#if DEBUG
        if (FusionDevMode.UnlockEverything)
#pragma warning disable CS0162 // Unreachable code detected
            return true;
#pragma warning restore CS0162 // Unreachable code detected
#endif

#pragma warning disable CS0162 // Unreachable code detected
        return _unlockedItems.Contains(barcode);
#pragma warning restore CS0162 // Unreachable code detected
    }

    public static bool IsEquipped(string barcode)
    {
        return _equippedItems.Contains(barcode);
    }

    public static void UnlockItem(string barcode)
    {
        if (!_unlockedItems.Contains(barcode))
            _unlockedItems.Add(barcode);

        WriteToFile();
    }

    public static void LockItem(string barcode)
    {
        _unlockedItems.Remove(barcode);

        WriteToFile();
    }

    public static void UpgradeItem(string barcode)
    {
        if (!_itemUpgrades.ContainsKey(barcode))
            _itemUpgrades.Add(barcode, -1);

        _itemUpgrades[barcode]++;
    }

    public static void SetUpgradeLevel(string barcode, int level)
    {
        if (!_itemUpgrades.ContainsKey(barcode))
            _itemUpgrades.Add(barcode, -1);

        _itemUpgrades[barcode] = level;
    }

    public static void SetEquipped(string barcode, bool isEquipped)
    {
        if (isEquipped)
        {
            if (!_equippedItems.Contains(barcode))
                _equippedItems.Add(barcode);
        }
        else
            _equippedItems.Remove(barcode);

        WriteToFile();
    }

    public static int GetBitCount() => _totalBits;

    public static void SetBitCount(int count)
    {
        _totalBits = Math.Max(0, count);
        WriteToFile();
    }

    private static List<string> _unlockedItems = new();
    private static List<string> _equippedItems = new();
    private static Dictionary<string, int> _itemUpgrades = new();
    private static int _totalBits;
}