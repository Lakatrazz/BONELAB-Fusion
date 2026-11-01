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
        string fullPath = PersistentData.GetAbsolutePath(path);

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

    public static void WriteJsonToFile<T>(string relativePath, T value)
    {
        string fullPath = PersistentData.GetAbsolutePath(relativePath);

        CreateDirectoryIfMissing(fullPath);

        string jsonText = JsonSerializer.Serialize(value, SerializerOptions);

        File.WriteAllText(fullPath, jsonText);
    }

    public static T ReadJsonFromFileWithBackup<T>(string relativePath)
    {
        if (TryReadJsonFromFile<T>(relativePath, out var value)) 
        {
            return value;
        }

        string fullPath = PersistentData.GetAbsolutePath(relativePath);
        string backupPath = fullPath + BackupSuffix;

        if (File.Exists(backupPath))
        {
            File.Copy(backupPath, fullPath, true);

            TryReadJsonFromFile(relativePath, out value);
            return value;
        }

        return default;
    }

    public static bool TryReadJsonFromFile<T>(string relativePath, out T value)
    {
        value = default;

        string fullPath = PersistentData.GetAbsolutePath(relativePath);

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
            FusionLogger.LogException($"reading save data at {relativePath}", e);
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<T>(jsonText, SerializerOptions);
            return true;
        }
        catch (Exception e)
        {
            FusionLogger.LogException($"deserializing save data at {relativePath}", e);
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