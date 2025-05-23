using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Scene;

public static class NetworkSceneManager
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

    /// <summary>
    /// Returns if the Local Player is the host of the active level.
    /// </summary>
    public static bool IsLevelHost => NetworkInfo.IsHost;

    /// <summary>
    /// Returns if the active level is networked. It will not be networked if there is no server or if <see cref="Purgatory"/> is enabled.
    /// </summary>
    public static bool IsLevelNetworked => NetworkInfo.HasServer && !Purgatory;

    /// <summary>
    /// Invoked when <see cref="Purgatory"/> changes.
    /// </summary>
    public static event Action<bool> OnPurgatoryChanged;

    /// <summary>
    /// Returns if this player is in the Local Player's current level.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static bool InCurrentLevel(PlayerId player)
    {
        if (player.IsMe)
        {
            return true;
        }

        if (!IsLevelNetworked)
        {
            return false;
        }

        return InLevel(player, FusionSceneManager.Barcode);
    }

    /// <summary>
    /// Returns if this player is in a specific level.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="barcode">The barcode of the level.</param>
    /// <returns></returns>
    public static bool InLevel(PlayerId player, string barcode)
    {
        return GetLevelBarcode(player) == barcode;
    }

    /// <summary>
    /// Gets the level barcode that a player is in.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns></returns>
    public static string GetLevelBarcode(PlayerId player)
    {
        return player.Metadata.LevelBarcode.GetValue();
    }
}
