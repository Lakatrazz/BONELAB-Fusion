using LabFusion.Marrow.Integration;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes;

/// <summary>
/// Functions for interacting with Gamemodes or implementing Gamemode functionality..
/// </summary>
public static class GamemodeHelper
{
    /// <summary>
    /// Starts a new server with a specific Gamemode.
    /// </summary>
    /// <param name="gamemode">The Gamemode to select upon starting the server.</param>
    public static void StartGamemodeServer(Gamemode gamemode)
    {
        if (NetworkInfo.HasServer)
        {
            NetworkHelper.Disconnect();
        }

        NetworkHelper.StartServer();

        DelayUtilities.Delay(() =>
        {
            FusionSceneManager.HookOnLevelLoad(() =>
            {
                GamemodeManager.SelectGamemode(gamemode);
            });
        }, 10);
    }

    /// <summary>
    /// Sets the Local Player's spawn point to a specific GamemodeMarker.
    /// </summary>
    /// <param name="marker">The marker to use as a spawn point.</param>
    public static void SetSpawnPoint(GamemodeMarker marker)
    {
        var transforms = new Transform[] { marker.transform };

        FusionPlayer.SetSpawnPoints(transforms);
    }

    /// <summary>
    /// Sets the Local Player's spawn point based on a list of GamemodeMarkers.
    /// </summary>
    /// <param name="markers">The markers to use for spawn points.</param>
    public static void SetSpawnPoints(IEnumerable<GamemodeMarker> markers)
    {
        var transforms = new Transform[markers.Count()];

        for (var i = 0; i < markers.Count(); i++)
        {
            transforms[i] = markers.ElementAt(i).transform;
        }

        FusionPlayer.SetSpawnPoints(transforms);
    }

    /// <summary>
    /// Resets the Local Player's spawn point overrides.
    /// </summary>
    public static void ResetSpawnPoints() => FusionPlayer.ResetSpawnPoints();

    /// <summary>
    /// Teleports the Local Player to a spawn point set with <see cref="SetSpawnPoints(IEnumerable{GamemodeMarker})"/>,
    /// or the level's checkpoint if none are set.
    /// </summary>
    public static void TeleportToSpawnPoint()
    {
        if (FusionPlayer.TryGetSpawnPoint(out var spawn))
        {
            LocalPlayer.TeleportToPosition(spawn.position);
        }
        else
        {
            LocalPlayer.TeleportToCheckpoint();
        }
    }
}
