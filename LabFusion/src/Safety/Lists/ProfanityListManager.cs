using LabFusion.Data;

using System.Text.Json.Serialization;

namespace LabFusion.Safety;

[Serializable]
public class ProfanityList
{
    [JsonPropertyName("words")]
    public List<string> Words { get; set; } = new();
}

public static class ProfanityListManager
{
    public const string FileName = "profanityList.json";

    public static ProfanityList List { get; private set; } = new();

    public static void FetchFile()
    {
        ListFetcher.FetchFile(FileName, OnFileFetched);
    }

    private static void OnFileFetched(string text)
    {
        List = DataSaver.ReadJsonFromText<ProfanityList>(text);
    }

    public static void ExportFile()
    {
        DataSaver.WriteJsonToFile(FileName, List);
    }

    public static void ExportWord(string word)
    {
        List.Words.Add(word);

        ExportFile();
    }
}
