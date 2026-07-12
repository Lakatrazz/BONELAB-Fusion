using System.Text.Json;

using LabFusion.Utilities;

namespace LabFusion.Data;

public static class JsonSaver
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true,
    };

    public static readonly string BackupSuffix = ".bak";

    public static void WriteJsonToFileWithBackup<T>(string path, T value)
    {
        string fullPath = PersistentData.GetPath(path);

        CreateDirectoryIfMissing(fullPath);

        string backupPath = fullPath + BackupSuffix;

        if (File.Exists(fullPath))
        {
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }

            File.Move(fullPath, backupPath);
        }

        WriteJsonToFile(path, value);
    }

    public static void WriteJsonToFile<T>(string path, T value)
    {
        string fullPath = PersistentData.GetPath(path);

        CreateDirectoryIfMissing(fullPath);

        string jsonText = JsonSerializer.Serialize(value, SerializerOptions);

        File.WriteAllText(fullPath, jsonText);
    }

    public static T ReadJsonFromFileWithBackup<T>(string path)
    {
        if (TryReadJsonFromFile<T>(path, out var value)) 
        {
            return value;
        }

        string fullPath = PersistentData.GetPath(path);
        string backupPath = fullPath + BackupSuffix;

        if (File.Exists(backupPath))
        {
            File.Copy(backupPath, fullPath, true);

            TryReadJsonFromFile(path, out value);
            return value;
        }

        return default;
    }

    public static bool TryReadJsonFromFile<T>(string path, out T value)
    {
        value = default;

        string fullPath = PersistentData.GetPath(path);

        if (!File.Exists(fullPath))
        {
            return false;
        }

        string jsonText;

        try
        {
            jsonText = File.ReadAllText(fullPath);
        }
        catch (UnauthorizedAccessException e)
        {
            FusionLogger.LogException($"reading save data at {path}", e);
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<T>(jsonText, SerializerOptions);
            return true;
        }
        catch (Exception e)
        {
            FusionLogger.LogException($"deserializing save data at {path}", e);
            return false;
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

    private static void CreateDirectoryIfMissing(string fullPath)
    {
        string directoryName = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
    }
}