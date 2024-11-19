namespace LabFusion.Data;

public static class ModBlacklist
{
    private static readonly List<string> _blacklistedModIds = new();

    public static void OnInitializeMelon()
    {
        CreateFile();
    }

    public static void ReadFile()
    {
        _blacklistedModIds.Clear();

        var path = GetFilePath();
        var lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
            {
                _blacklistedModIds.Add(line.Trim());
            }
        }
    }

    public static void CreateFile()
    {
        var path = GetFilePath();

        if (!File.Exists(path))
        {
            using StreamWriter sw = File.CreateText(path);

            sw.WriteLine("# This file is for preventing the use of specific mods while in a server.");
            sw.WriteLine("# To blacklist a mod, add a mod identifier on its own line.");
            sw.WriteLine("# A mod identifier can either be its mod.io number ID, mod.io URL ID, or AssetWarehouse barcode.");
            sw.WriteLine("# When the identifier is a number ID or URL ID, it will block the mod from being downloaded.");
            sw.WriteLine("# On the other hand, if the identifier is a AssetWarehouse barcode, it will prevent the mod from being used.");
            sw.WriteLine("# For server hosts, blacklisting a barcode prevents all clients from using that mod.");
            sw.WriteLine("#");
            sw.WriteLine("# Examples");
            sw.WriteLine("# -------------------------------------");
            sw.WriteLine("# Number ID");
            sw.WriteLine("# 4057308");
            sw.WriteLine("#");
            sw.WriteLine("# URL ID");
            sw.WriteLine("# test-chambers");
            sw.WriteLine("#");
            sw.WriteLine("# Barcode");
            sw.WriteLine("# SLZ.TestChambers.Level.TestChamber02");
            sw.WriteLine("# SLZ.TestChambers.Level.TestChamber07");
            sw.WriteLine("# -------------------------------------");
        }
    }

    public static string GetFilePath()
    {
        return PersistentData.GetPath("mod_blacklist.txt");
    }

    public static bool IsBlacklisted(string identifier)
    {
        foreach (var blacklisted in _blacklistedModIds)
        {
            if (blacklisted == identifier)
            {
                return true;
            }
        }

        return false;
    }
}
