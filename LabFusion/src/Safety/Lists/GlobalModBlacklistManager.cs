using LabFusion.Data;

using System.Text.Json.Serialization;

namespace LabFusion.Safety;

[Serializable]
public class GlobalModBlacklist
{
    [JsonPropertyName("mods")]
    public List<ModInfo> Mods { get; set; } = new();
}

public static class GlobalModBlacklistManager
{
    public const string FileName = "globalModBlacklist.json";

    public static GlobalModBlacklist List { get; private set; } = new();

    public static void FetchFile()
    {
        ListFetcher.FetchFile(FileName, OnFileFetched);
    }

    private static void OnFileFetched(string text)
    {
        List = DataSaver.ReadJsonFromText<GlobalModBlacklist>(text);
    }

    public static void ExportFile()
    {
        DataSaver.WriteJsonToFile(FileName, List);
    }

    public static bool IsBarcodeBlacklisted(string barcode)
    {
        return List.Mods.Any(m => m.Barcodes.Contains(barcode));
    }

    public static bool IsModIDBlacklisted(int modID)
    {
        return List.Mods.Any(m => m.ModID != -1 && m.ModID == modID);
    }

    public static bool IsNameIDBlacklisted(string nameID)
    {
        return List.Mods.Any(m => !string.IsNullOrWhiteSpace(m.NameID) && m.NameID == nameID);
    }
}
