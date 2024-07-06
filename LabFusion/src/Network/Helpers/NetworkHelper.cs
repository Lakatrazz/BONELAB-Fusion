﻿using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Representation;

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
        NetworkInfo.CurrentNetworkLayer?.StartServer();
    }

    /// <summary>
    /// Stops an existing server.
    /// </summary>
    public static void StopServer()
    {
        if (NetworkInfo.CurrentNetworkLayer != null && NetworkInfo.IsServer)
        {
            NetworkInfo.CurrentNetworkLayer.Disconnect();
        }
    }

    /// <summary>
    /// Disconnects the network layer and cleans up.
    /// </summary>
    public static void Disconnect(string reason = "")
    {
        NetworkInfo.CurrentNetworkLayer?.Disconnect(reason);
    }

    /// <summary>
    /// Pushes an update to the lobby metadata.
    /// </summary>
    public static void UpdateLobby()
    {
        InternalLayerHelpers.OnUpdateLobby();
    }

    /// <summary>
    /// Returns true if this user is friended on the active network platform.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static bool IsFriend(ulong userId)
    {
        if (NetworkInfo.CurrentNetworkLayer != null)
            return NetworkInfo.CurrentNetworkLayer.IsFriend(userId);

        return false;
    }

    /// <summary>
    /// Kicks a user from the game.
    /// </summary>
    /// <param name="id"></param>
    public static void KickUser(PlayerId id)
    {
        // Don't kick blessed users
        if (FusionBlessings.IsBlessed(id))
        {
            if (!id.TryGetDisplayName(out var name))
                name = "Wacky Willy";

            FusionNotifier.Send(new FusionNotification()
            {
                title = "Failed to Kick User",
                showTitleOnPopup = true,

                message = $"{name} has denied your kick request.",

                isMenuItem = false,
                isPopup = true,
                type = NotificationType.ERROR,
            });

            return;
        }

        ConnectionSender.SendDisconnect(id, "Kicked from Server");
    }

    /// <summary>
    /// Bans a user from the game.
    /// </summary>
    /// <param name="id"></param>
    public static void BanUser(PlayerId id)
    {
        // Don't ban blessed users
        if (FusionBlessings.IsBlessed(id))
        {
            if (!id.TryGetDisplayName(out var name))
                name = "Wacky Willy";

            FusionNotifier.Send(new FusionNotification()
            {
                title = "Failed to Ban User",
                showTitleOnPopup = true,

                message = $"{name} has denied your ban request.",

                isMenuItem = false,
                isPopup = true,
                type = NotificationType.ERROR,
            });

            return;
        }

        BanList.Ban(id.LongId, id.Metadata.GetMetadata(MetadataHelper.UsernameKey), "Banned");
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
        foreach (var tuple in BanList.BannedUsers)
        {
            if (tuple.Item1 == longId)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Pardons a user from the ban list.
    /// </summary>
    /// <param name="longId"></param>
    public static void PardonUser(ulong longId)
    {
        BanList.Pardon(longId);
    }
}