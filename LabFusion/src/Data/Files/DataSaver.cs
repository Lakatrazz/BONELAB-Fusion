using System.Text.Json;

using LabFusion.Utilities;

namespace LabFusion.Data;

public static class DataSaver
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true,
    };

    public static void WriteJsonToFile<T>(string path, T value)
    {
        string fullPath = PersistentData.GetPath(path);
        string directoryName = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        string jsonText = JsonSerializer.Serialize(value, SerializerOptions);

        File.WriteAllText(fullPath, jsonText);
    }

    public static T ReadJsonFromFile<T>(string path)
    {
        string fullPath = PersistentData.GetPath(path);

        if (!File.Exists(fullPath))
        {
            return default;
        }

        string jsonText;

        try
        {
            jsonText = File.ReadAllText(fullPath);
        }
        catch (UnauthorizedAccessException e)
        {
            FusionLogger.LogException($"reading save data at {path}", e);
            return default;
        }

        try
        {
            T result = JsonSerializer.Deserialize<T>(jsonText, SerializerOptions);
            return result;
        }
        catch (Exception e)
        {
            FusionLogger.LogException($"deserializing save data at {path}", e);

            File.Delete(fullPath);
            return default;
        }
    }

    public static T ReadJsonFromText<T>(string text)
    {
        try
        {
            T result = JsonSerializer.Deserialize<T>(text, SerializerOptions);
            return result;
        }
        catch (Exception e)
        {
            FusionLogger.LogException($"deserializing json from text", e);
            return default;
        }
    }
}