using LabFusion.BoneMenu;
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
        if (GamemodeTypes.Contains(type))
        {
            throw new ArgumentException($"Gamemode {type.Name} was already registered.");
        }

        GamemodeTypes.Add(type);
    }

    public static string[] GetExistingTypeNames()
    {
        string[] array = new string[GamemodeTypes.Count];
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = GamemodeTypes[i].AssemblyQualifiedName;
        }
        return array;
    }

    public static FusionDictionary<string, string>[] GetExistingMetadata()
    {
        FusionDictionary<string, string>[] metadata = new FusionDictionary<string, string>[Gamemodes.Length];

        for (var i = 0; i < metadata.Length; i++)
        {
            metadata[i] = Gamemodes[i].Metadata.LocalDictionary;
        }

        return metadata;
    }

    public static void PopulateGamemodeTable(string[] names)
    {
        Gamemodes = new Gamemode[names.Length];

        for (ushort i = 0; i < names.Length; i++)
        {
            var type = Type.GetType(names[i]);
            if (type != null && GamemodeTypes.Contains(type))
            {
                var handler = Internal_CreateGamemode(type, i);
                Gamemodes[i] = handler;
                handler.GamemodeRegistered();
            }
        }

        BoneMenuCreator.RefreshGamemodes();
    }

    public static void PopulateGamemodeMetadatas(FusionDictionary<string, string>[] metadatas)
    {
        for (var i = 0; i < Gamemodes.Length && i < metadatas.Length; i++)
        {
            var gamemode = Gamemodes[i];
            var metadata = metadatas[i];

            if (gamemode != null && metadata != null)
            {
                foreach (var pair in metadata)
                {
                    gamemode.Metadata.ForceSetLocalMetadata(pair.Key, pair.Value);
                }
            }
        }
    }

    public static void ClearGamemodeTable()
    {
        // Force stop gamemodes
        if (Gamemodes != null && Gamemodes.Length > 0)
        {
            foreach (var gamemode in Gamemodes)
            {
                if (gamemode == null)
                    continue;

                gamemode.Internal_SetGamemodeState(false);
                gamemode.GamemodeUnregistered();
            }
        }

        GamemodeManager.Internal_SetActiveGamemode(null);
        BoneMenuCreator.ClearGamemodes();

        Gamemodes = null;
    }

    private static Gamemode Internal_CreateGamemode(Type type, ushort tag)
    {
        var gamemode = Activator.CreateInstance(type) as Gamemode;
        gamemode._tag = tag;
        return gamemode;
    }

    public static ushort? GetGamemodeTag(Type type)
    {
        if (Gamemodes != null)
        {
            for (ushort i = 0; i < Gamemodes.Length; i++)
            {
                var other = Gamemodes[i];
                if (other.GetType() == type)
                    return i;
            }
        }

        return null;
    }


    public static readonly List<Type> GamemodeTypes = new();
    public static Gamemode[] Gamemodes { get; private set; } = null;
}