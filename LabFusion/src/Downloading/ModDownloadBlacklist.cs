using LabFusion.Data;

namespace LabFusion.Downloading;

public static class ModDownloadBlacklist
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

            sw.WriteLine("# This file is for preventing certain mods from being automatically downloaded.");
            sw.WriteLine("# To blacklist a mod, add a mod id on its own line.");
            sw.WriteLine("# A mod id can either be its number id, url id, or barcode.");
            sw.WriteLine("# Any lines starting with a # are ignored.");
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

    public static bool IsBlacklisted(string modId)
    {
        foreach (var blacklisted in _blacklistedModIds)
        {
            if (blacklisted == modId)
            {
                return true;
            }
        }

        return false;
    }
}
