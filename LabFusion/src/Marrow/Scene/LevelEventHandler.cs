using System.Reflection;

using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Marrow.Scene;

/// <summary>
/// An inheritable class that lets you receive callbacks when a specific level is loaded.
/// </summary>
public abstract class LevelEventHandler
{
    /// <summary>
    /// The barcode of the level for this level event. If this should work on all levels, leave as null.
    /// </summary>
    public virtual string LevelBarcode => null;

    /// <summary>
    /// Returns if the LevelEventHandler supports the current level.
    /// </summary>
    /// <returns></returns>
    public bool IsInLevel()
    {
        if (string.IsNullOrWhiteSpace(LevelBarcode))
        {
            return true;
        }

        return FusionSceneManager.Barcode == LevelBarcode;
    }

    /// <summary>
    /// Invoked when the level is finished loading.
    /// </summary>
    protected virtual void OnLevelLoaded() { }

    /// <summary>
    /// Invoked when a player needs to be caught up for the current level.
    /// </summary>
    /// <param name="playerID"></param>
    protected virtual void OnPlayerCatchup(PlayerID playerID) { }

    private static void OnLevelLoadedCallback()
    {
        for (var i = 0; i < Handlers.Count; i++)
        {
            var handler = Handlers[i];

            if (!handler.IsInLevel())
            {
                continue;
            }

            handler.OnLevelLoaded();
        }
    }

    private static void OnPlayerCatchupCallback(PlayerID playerID)
    {
        for (var i = 0; i < Handlers.Count; i++)
        {
            var handler = Handlers[i];

            if (!handler.IsInLevel())
            {
                continue;
            }

            handler.OnPlayerCatchup(playerID);
        }
    }

    public static void OnInitializeMelon()
    {
        // Hook functions
        MultiplayerHooking.OnMainSceneInitialized += OnLevelLoadedCallback;
        CatchupManager.OnPlayerServerCatchup += OnPlayerCatchupCallback;

        // Register all of our handlers
        LoadHandlers(FusionMod.FusionAssembly);
    }

    public static void LoadHandlers(Assembly assembly)
    {
        if (assembly == null) 
        { 
            throw new NullReferenceException("Can't register from a null assembly!"); 
        }

#if DEBUG
        FusionLogger.Log($"Populating LevelDataHandler list from {assembly.GetName().Name}!");
#endif

        AssemblyUtilities.LoadAllValid<LevelEventHandler>(assembly, RegisterHandler);
    }

    public static void RegisterHandler<T>() where T : NativeMessageHandler => RegisterHandler(typeof(T));

    protected static void RegisterHandler(Type type)
    {
        // Create the handler
        LevelEventHandler handler = Activator.CreateInstance(type) as LevelEventHandler;
        Handlers.Add(handler);

#if DEBUG
        FusionLogger.Log($"Registered {type.Name}");
#endif
    }

    public static readonly List<LevelEventHandler> Handlers = new();
}