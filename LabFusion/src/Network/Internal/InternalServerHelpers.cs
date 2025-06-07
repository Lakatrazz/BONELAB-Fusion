using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Preferences;
using LabFusion.SDK.Points;
using LabFusion.SDK.Achievements;
using LabFusion.SDK.Modules;
using LabFusion.UI.Popups;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.SceneStreaming;

namespace LabFusion.Network;

/// <summary>
/// Internal class used for cleaning up servers, executing events on disconnect, etc.
/// </summary>
public static class InternalServerHelpers
{
    private static void DisposeUser(PlayerID id)
    {
        id?.Cleanup();
    }

    private static void DisposeUsers()
    {
        foreach (var id in PlayerIDManager.PlayerIDs.ToList())
        {
            DisposeUser(id);
        }
    }

    /// <summary>
    /// Initializes information about the server, such as module types.
    /// </summary>
    public static void OnStartServer()
    {
        // Apply initial metadata
        LocalPlayer.InvokeApplyInitialMetadata();

        // Create local id
        var id = new PlayerID(PlayerIDManager.LocalPlatformID, 0, LocalPlayer.Metadata.Metadata.LocalDictionary, GetInitialEquippedItems());
        id.Insert();
        PlayerIDManager.ApplyLocalID();

        NetworkPlayerManager.CreateLocalPlayer();

        // Update hooks
        MultiplayerHooking.InvokeOnStartedServer();

        NetworkNotifications.SendStartedServerNotification();

        // Unlock achievement
        if (AchievementManager.TryGetAchievement<HeadOfHouse>(out var achievement))
        {
            achievement.IncrementTask();
        }

        // Reload the scene
        SceneStreamer.Reload();
    }

    /// <summary>
    /// Called when the user joins a server.
    /// </summary>
    public static void OnJoinServer()
    {
        // Send settings
        FusionPreferences.SendClientSettings();

        // Update hooks
        MultiplayerHooking.InvokeOnJoinedServer();

        NetworkNotifications.SendJoinedServerNotification();

        // Unlock achievement
        if (AchievementManager.TryGetAchievement<WarmWelcome>(out var achievement))
            achievement.IncrementTask();
    }

    /// <summary>
    /// Cleans up the scene from all users. ONLY call this from within a network layer!
    /// </summary>
    public static void OnDisconnect(string reason = "")
    {
        // Cleanup information
        DisposeUsers();
        NetworkEntityManager.OnCleanupEntities();

        // Update hooks
        MultiplayerHooking.InvokeOnDisconnected();

        NetworkNotifications.SendDisconnectedNotification(reason);
    }

    /// <summary>
    /// Updates information about the new user.
    /// </summary>
    /// <param name="id"></param>
    public static void OnPlayerJoined(PlayerID id, bool isInitialJoin)
    {
        // Send client info
        FusionPreferences.SendClientSettings();

        // Update layer
        InternalLayerHelpers.OnUserJoin(id);

        // Update hooks
        MultiplayerHooking.InvokeOnPlayerJoined(id);

        // Send notification
        if (isInitialJoin && id.TryGetDisplayName(out var name))
        {
            NetworkNotifications.SendPlayerJoinedNotification(name);
        }
    }

    /// <summary>
    /// Cleans up a single user after they have left.
    /// </summary>
    /// <param name="longId"></param>
    public static void OnPlayerLeft(ulong longId)
    {
        var playerId = PlayerIDManager.GetPlayerID(longId);

        // Make sure the player exists in our game
        if (playerId == null)
            return;

        // Send notification
        if (playerId.TryGetDisplayName(out var name))
        {
            NetworkNotifications.SendPlayerLeftNotification(name);
        }

        DisposeUser(playerId);

        MultiplayerHooking.InvokeOnPlayerLeft(playerId);
    }

    /// <summary>
    /// Gets the default list of equipped items.
    /// </summary>
    /// <returns></returns>
    public static List<string> GetInitialEquippedItems()
    {
        var list = new List<string>();

        foreach (var item in PointItemManager.LoadedItems)
        {
            if (item.IsEquipped)
                list.Add(item.Barcode);
        }

        return list;
    }
}