using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Representation;
using LabFusion.UI.Popups;

namespace LabFusion.Network;

/// <summary>
/// Helper class for calling basic methods on the Server or Client.
/// </summary>
public static class NetworkHelper
{
    /// <summary>
    /// Starts a server if there is currently none active.
    /// </summary>
    public static void StartServer()
    {
        NetworkLayerManager.Layer?.StartServer();
    }

    /// <summary>
    /// Disconnects the network layer and cleans up.
    /// </summary>
    public static void Disconnect(string reason = "")
    {
        NetworkLayerManager.Layer?.Disconnect(reason);
    }

    /// <summary>
    /// Attempts to join a server given a server code.
    /// </summary>
    /// <param name="code"></param>
    public static void JoinServerByCode(string code)
    {
        NetworkLayerManager.Layer?.JoinServerByCode(code);
    }

    /// <summary>
    /// Gets the code of the current server.
    /// </summary>
    /// <returns>The server code.</returns>
    public static string GetServerCode()
    {
        var layer = NetworkLayerManager.Layer;

        if (layer == null)
        {
            return null;
        }

        return layer.GetServerCode();
    }

    /// <summary>
    /// Generates a new server code.
    /// </summary>
    public static void RefreshServerCode()
    {
        NetworkLayerManager.Layer.RefreshServerCode();
    }

    /// <summary>
    /// Returns true if this user is friended on the active network platform.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static bool IsFriend(ulong userId)
    {
        if (NetworkLayerManager.Layer != null)
            return NetworkLayerManager.Layer.IsFriend(userId);

        return false;
    }

    /// <summary>
    /// Kicks a user from the game.
    /// </summary>
    /// <param name="id"></param>
    public static void KickUser(PlayerID id)
    {
        // Don't kick blessed users
        if (FusionBlessings.IsBlessed(id))
        {
            if (!id.TryGetDisplayName(out var name))
                name = "Wacky Willy";

            Notifier.Send(new Notification()
            {
                Title = "Failed to Kick User",

                Message = $"{name} has denied your kick request.",

                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.ERROR,
            });

            return;
        }

        ConnectionSender.SendDisconnect(id, "Kicked from Server");
    }

    /// <summary>
    /// Bans a user from the game.
    /// </summary>
    /// <param name="id"></param>
    public static void BanUser(PlayerID id)
    {
        // Don't ban blessed users
        if (FusionBlessings.IsBlessed(id))
        {
            if (!id.TryGetDisplayName(out var name))
                name = "Wacky Willy";

            Notifier.Send(new Notification()
            {
                Title = "Failed to Ban User",

                Message = $"{name} has denied your ban request.",

                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.ERROR,
            });

            return;
        }

        BanManager.Ban(new PlayerInfo(id), "Banned");
        ConnectionSender.SendDisconnect(id, "Banned from Server");
    }

    /// <summary>
    /// Checks if a user is banned.
    /// </summary>
    /// <param name="longId"></param>
    /// <returns></returns>
    public static bool IsBanned(ulong longId)
    {
        // Check if the user is blessed
        if (FusionBlessings.IsBlessed(longId))
            return false;

        // Check the ban list
        foreach (var ban in BanManager.BanList.Bans)
        {
            if (ban.Player.LongId == longId)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Pardons a user from the ban list.
    /// </summary>
    /// <param name="longId"></param>
    public static void PardonUser(ulong longId)
    {
        BanManager.Pardon(longId);
    }
}