using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.UI.Popups;
using LabFusion.Extensions;

using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow;

using UnityEngine;

namespace LabFusion.Utilities;

public static class FusionPlayer
{
    public static byte? LastAttacker { get; internal set; }
    public static readonly List<Transform> SpawnPoints = new();

    private static bool _brokeBounds = false;

    internal static void OnMainSceneInitialized()
    {
        LastAttacker = null;

        if (_brokeBounds)
        {
            Physics.autoSimulation = true;
            _brokeBounds = false;
        }
    }

    internal static void OnUpdate()
    {
        if (FusionSceneManager.IsLoading())
        {
            return;
        }

        if (!RigData.HasPlayer)
        {
            return;
        }

        CheckFloatingPoint();
    }

    private static void CheckFloatingPoint()
    {
        var rm = RigData.Refs.RigManager;
        var position = rm.physicsRig.feet.transform.position;

        if (NetworkTransformManager.IsInBounds(position))
        {
            return;
        }

#if DEBUG
        FusionLogger.Warn("Player was sent out of bounds, reloading scene.");
#endif

        // Incase we hit NaN, don't simulate physics!
        Physics.autoSimulation = false;
        _brokeBounds = true;

        if (NetworkInfo.HasServer && !NetworkInfo.IsHost)
        {
            NetworkHelper.Disconnect("Left Bounds");
        }

        SceneStreamer.Reload();

        Notifier.Send(new Notification()
        {
            ShowPopup = true,
            Title = "Whoops! Sorry about that!",
            Type = NotificationType.WARNING,
            Message = "The scene was reloaded due to being sent far out of bounds.",
            PopupLength = 6f,
        });
    }

    /// <summary>
    /// Tries to get the player that we were last attacked by.
    /// </summary>
    /// <returns></returns>
    public static bool TryGetLastAttacker(out PlayerID id)
    {
        id = null;

        if (!LastAttacker.HasValue)
            return false;

        id = PlayerIDManager.GetPlayerID(LastAttacker.Value);
        return id != null;
    }

    /// <summary>
    /// Checks if the RigManager is the local player.
    /// </summary>
    /// <param name="rigManager"></param>
    /// <returns></returns>
    public static bool IsLocalPlayer(this RigManager rigManager)
    {
        if (!RigData.HasPlayer)
        {
            return true;
        }

        return rigManager == RigData.Refs.RigManager;
    }

    /// <summary>
    /// Sets the custom spawn points for the player.
    /// </summary>
    /// <param name="points"></param>
    public static void SetSpawnPoints(params Transform[] points)
    {
        SpawnPoints.Clear();
        SpawnPoints.AddRange(points);
    }

    /// <summary>
    /// Clears all spawn points.
    /// </summary>
    public static void ResetSpawnPoints()
    {
        SpawnPoints.Clear();
    }

    /// <summary>
    /// Gets a random spawn point from the list.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public static bool TryGetSpawnPoint(out Transform point)
    {
        point = null;

        SpawnPoints.RemoveAll((t) => t == null);

        if (SpawnPoints.Count > 0)
        {
            point = SpawnPoints.GetRandom();
            return true;
        }

        return false;
    }
}