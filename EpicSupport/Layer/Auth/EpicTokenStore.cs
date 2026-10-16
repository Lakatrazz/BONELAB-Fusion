using LabFusion.Data;

namespace MarrowFusion.Epic;

internal static class EpicTokenStore
{
    private const string FileName = "epic_token.txt";

    internal static string Get()
    {
        var path = PersistentData.GetPath(FileName);
        
        try
        {
            if (!File.Exists(path))
                return null;
 
            string token = File.ReadAllText(path).Trim();
            return token.Length == 0 ? null : token;
        }
        catch
        {
            return null;
        }
    }

    internal static void Set(string token)
    {
        if (string.IsNullOrEmpty(token))
            return;
        
        var path = PersistentData.GetPath(FileName);
        
        try
        {
            File.WriteAllText(path, token);
        }
        catch
        {
            // Ignore errors
        }
    }
    
    internal static void Clear()
    {
        var path = PersistentData.GetPath(FileName);
        
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Ignore errors
        }
    }
}