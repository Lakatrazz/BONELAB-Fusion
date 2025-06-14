using LabFusion.Data;
using LabFusion.Utilities;

using System.Reflection;

namespace LabFusion.SDK.Gamemodes;

public static class GamemodeRegistration
{
    public static void LoadGamemodes(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new NullReferenceException("Tried loading gamemodes from a null assembly!");
        }

        AssemblyUtilities.LoadAllValid<Gamemode>(assembly, RegisterGamemode);
    }

    public static void RegisterGamemode<T>() where T : Gamemode => RegisterGamemode(typeof(T));

    private static void RegisterGamemode(Type type)
    {
        var gamemodeInstance = Activator.CreateInstance(type) as Gamemode;
        gamemodeInstance.GamemodeRegistered();

        Gamemodes.Add(gamemodeInstance);
        GamemodeLookup.Add(gamemodeInstance.Barcode, gamemodeInstance);
    }

    public static Dictionary<string, Dictionary<string, string>> GetExistingMetadata()
    {
        Dictionary<string, Dictionary<string, string>> metadata = new();

        for (var i = 0; i < Gamemodes.Count; i++)
        {
            var gamemode = Gamemodes[i];

            // Only get metadata for selected gamemodes
            // Any other gamemodes are unnecessary
            if (!gamemode.IsSelected)
            {
                continue;
            }

            var barcode = gamemode.Barcode;

            var metadataPairs = new Dictionary<string, string>();

            foreach (var pair in gamemode.Metadata.LocalDictionary)
            {
                metadataPairs.Add(pair.Key, pair.Value);
            }

            metadata.Add(barcode, metadataPairs);
        }

        return metadata;
    }

    public static bool TryGetGamemode(string barcode, out Gamemode gamemode)
    {
        return GamemodeLookup.TryGetValue(barcode, out gamemode);
    }

    public static void PopulateGamemodeMetadatas(Dictionary<string, Dictionary<string, string>> metadatas)
    {
        foreach (var pair in metadatas)
        {
            var barcode = pair.Key;

            if (!TryGetGamemode(barcode, out var gamemode))
            {
                continue;
            }

            var metadata = pair.Value;

            foreach (var metadataPair in metadata)
            {
                gamemode.Metadata.ForceSetLocalMetadata(metadataPair.Key, metadataPair.Value);
            }
        }
    }

    public static List<Gamemode> Gamemodes { get; private set; } = new();
    public static Dictionary<string, Gamemode> GamemodeLookup { get; private set; } = new();
}