using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Data;

/// <summary>
/// Functions for getting paths within Fusion's persistent data folder.
/// </summary>
public static class PersistentData
{
    /// <summary>
    /// The path to the persistent data folder.
    /// </summary>
    public static string PersistentPath { get; private set; } = null;

    /// <summary>
    /// Whether <see cref="PersistentPath"/> has been initialized yet.
    /// </summary>
    public static bool Initialized { get; private set; } = false;

    /// <summary>
    /// Gets an absolute path in the persistent data folder from a relative path.
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    public static string GetAbsolutePath(string relativePath)
    {
        if (!Initialized)
        {
#if DEBUG
            FusionLogger.Warn("Tried getting a persistent path before it was initialized!\n" +
                "Forcing initialization!");
#endif

            InitializePath();
        }

        return Path.Combine(PersistentPath, relativePath);
    }

    internal static void InitializePath()
    {
        if (Initialized)
        {
            return;
        }

        string appData = Application.persistentDataPath;
        PersistentPath = Path.Combine(appData, ResourcePaths.PersistentDataRelativePath);

        FusionLogger.Log($"Data is at {PersistentPath}.", ConsoleColor.DarkCyan);
        ValidateDirectory(PersistentPath);

        Initialized = true;
    }

    private static void ValidateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}

