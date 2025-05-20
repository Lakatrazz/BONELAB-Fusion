using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Preferences;
using LabFusion.Preferences.Server;

using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Points;
using LabFusion.SDK.Achievements;
using LabFusion.SDK.Modules;

using LabFusion.Data;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.SceneStreaming;

namespace LabFusion.Network;

/// <summary>
/// Internal class used for cleaning up servers, executing events on disconnect, etc.
/// </summary>
public static class InternalServerHelpers
{
    private static void DisposeUser(PlayerId id)
    {
        id?.Cleanup();
    }

    private static void DisposeUsers()
    {
        foreach (var id in PlayerIdManager.PlayerIds.ToList())
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
        var id = new PlayerId(PlayerIdManager.LocalLongId, 0, LocalPlayer.Metadata.LocalDictionary, GetInitialEquippedItems());
        id.Insert();
        PlayerIdManager.ApplyLocalId();

        NetworkPlayerManager.CreateLocalPlayer();

        // Register module message handlers so they can send messages
        var names = ModuleMessageHandler.GetExistingTypeNames();
        ModuleMessageHandler.PopulateHandlerTable(names);

        // Update hooks
        MultiplayerHooking.InvokeOnStartedServer();

        // Send a notification
        FusionNotifier.Send(new FusionNotification()
        {
            Title = "Started Server",
            Message = "Started a server!",
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.SUCCESS,
        });

        // Unlock achievement
        if (AchievementManager.TryGetAchievement<HeadOfHouse>(out var achievement))
            achievement.IncrementTask();

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

        // Send a notification
        FusionNotifier.Send(new FusionNotification()
        {
            Title = "Joined Server",
            Message = "Joined a server!",
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.SUCCESS,
        });

        // Unlock achievement
        if (AchievementManager.TryGetAchievement<WarmWelcome>(out var achievement))
            achievement.IncrementTask();
    }

    /// <summary>
    /// Cleans up the scene from all users. ONLY call this from within a network layer!
    /// </summary>
    public static void OnDisconnect(string reason = "")
    {
        // Cleanup modules
        ModuleMessageHandler.ClearHandlerTable();

        // Cleanup information
        DisposeUsers();
        NetworkEntityManager.OnCleanupEntities();

        // Update hooks
        MultiplayerHooking.InvokeOnDisconnected();

        // Send a notification
        if (string.IsNullOrWhiteSpace(reason))
        {
            FusionNotifier.Send(new FusionNotification()
            {
                Title = "Disconnected from Server",
                Message = "Disconnected from the current server!",
                SaveToMenu = false,
                ShowPopup = true,
            });
        }
        else
        {
            FusionNotifier.Send(new FusionNotification()
            {
                Title = "Disconnected from Server",
                Message = $"You were disconnected for reason: {reason}",
                SaveToMenu = true,
                ShowPopup = true,
                PopupLength = 5f,
                Type = NotificationType.WARNING,
            });
        }
    }

    /// <summary>
    /// Updates information about the new user.
    /// </summary>
    /// <param name="id"></param>
    public static void OnUserJoin(PlayerId id, bool isInitialJoin)
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
            FusionNotifier.Send(new FusionNotification()
            {
                Title = $"{name} Joined",
                Message = $"{name} joined the server.",
                SaveToMenu = false,
                ShowPopup = true,
            });
        }
    }

    /// <summary>
    /// Cleans up a single user after they have left.
    /// </summary>
    /// <param name="longId"></param>
    public static void OnUserLeave(ulong longId)
    {
        var playerId = PlayerIdManager.GetPlayerId(longId);

        // Make sure the player exists in our game
        if (playerId == null)
            return;

        // Send notification
        if (playerId.TryGetDisplayName(out var name))
        {
            FusionNotifier.Send(new FusionNotification()
            {
                Title = $"{name} Left",
                Message = $"{name} left the server.",
                SaveToMenu = false,
                ShowPopup = true,
            });
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