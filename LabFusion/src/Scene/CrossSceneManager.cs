using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Scene;

public static class CrossSceneManager
{
    private static bool _purgatory = false;

    /// <summary>
    /// Determines if the player is in a "purgatory" state in the server.
    /// This means they are basically cut off from anything syncing such as spawnables, other players, events, etc.
    /// This is useful in cases such as a loading scene where you want the player to be able to use props but not interact with other players.
    /// </summary>
    public static bool Purgatory
    {
        get
        {
            return _purgatory;
        }
        set
        {
            if (value == _purgatory)
            {
                return;
            }

            _purgatory = value;

            OnPurgatoryChanged?.Invoke(value);
        }
    }

    public static event Action<bool> OnPurgatoryChanged;

    // In the future, have fake hosts per scene to control events
    // These will be determined by the first person to load in if cross scene is enabled
    // But for now, just use the global host
    public static bool IsSceneHost()
    {
        return NetworkInfo.IsHost;
    }

    /// <summary>
    /// Checks if the current scene should not be synced (usually if there is no server, or if this is in <see cref="Purgatory"/>.
    /// </summary>
    /// <returns>If the scene is unsynced.</returns>
    public static bool InUnsyncedScene()
    {
        return !NetworkInfo.HasServer || Purgatory;
    }

    public static bool InCurrentScene(PlayerId player)
    {
        if (player.IsMe)
        {
            return true;
        }

        if (InUnsyncedScene())
        {
            return false;
        }

        return true;
    }

    public static bool InScene(PlayerId player, string barcode)
    {
        return true;
    }

    public static string GetScene(PlayerId player)
    {
        return FusionSceneManager.Level.Barcode.ID;
    }
}
