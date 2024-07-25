using BoneLib.BoneMenu;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Senders;

using UnityEngine;

using System.Windows.Forms;

namespace LabFusion.BoneMenu;

public static partial class BoneMenuCreator
{
    private static Page _playerListCategory;

    public static void CreatePlayerListMenu(Page page)
    {
        // Root category
        _playerListCategory = page.CreatePage("Player List", Color.white);
        _playerListCategory.CreateFunction("Refresh", Color.white, RefreshPlayerList);
        _playerListCategory.CreateFunction("Select Refresh to load players!", Color.yellow, null);
    }

    private static void RefreshPlayerList()
    {
        // Clear existing lobbies
        _playerListCategory.RemoveAll();
        _playerListCategory.CreateFunction("Refresh", Color.white, RefreshPlayerList);

        // Add an item for every player
        foreach (var id in PlayerIdManager.PlayerIds)
        {
            CreatePlayer(id);
        }

        BoneLib.BoneMenu.Menu.OpenPage(_playerListCategory);
    }

    private static void CreatePlayer(PlayerId id)
    {
        // Get the name for the category
        string username = id.Metadata.GetMetadata(MetadataHelper.UsernameKey);
        string nickname = id.Metadata.GetMetadata(MetadataHelper.NicknameKey);

        username = username.LimitLength(PlayerIdManager.MaxNameLength);
        nickname = nickname.LimitLength(PlayerIdManager.MaxNameLength);

        string display;

        if (string.IsNullOrWhiteSpace(nickname))
            display = username;
        else
            display = $"{nickname} ({username})";

        // Get the current permission
        FusionPermissions.FetchPermissionLevel(id.LongId, out var level, out Color color);

        // Create the category and setup its options
        var category = _playerListCategory.CreatePage(display, color);

        ulong longId = id.LongId;
        byte smallId = id.SmallId;

        // Set permission display
        if (NetworkInfo.IsServer && !id.IsOwner)
        {
            var permSetter = category.CreateEnum($"Permissions", Color.yellow, level, (v) =>
            {
                FusionPermissions.TrySetPermission(longId, username, (PermissionLevel)v);
            });

            id.Metadata.OnMetadataChanged += (key, value) =>
            {
                if (key != MetadataHelper.PermissionKey)
                {
                    return;
                }

                if (!Enum.TryParse(value, out PermissionLevel newLevel))
                {
                    return;
                }

                permSetter.Value = newLevel;
            };
        }
        else
        {
            var permDisplay = category.CreateFunction($"Permissions: {level}", Color.yellow, null);

            id.Metadata.OnMetadataChanged += (key, value) =>
            {
                if (key != MetadataHelper.PermissionKey)
                {
                    return;
                }

                permDisplay.ElementName = $"Permissions: {value}";
            };
        }

        // Get self permissions
        FusionPermissions.FetchPermissionLevel(PlayerIdManager.LocalLongId, out var selfLevel, out _);

        var serverSettings = FusionPreferences.ActiveServerSettings;

        // Create vote options
        if (!id.IsOwner && FusionPermissions.HasSufficientPermissions(selfLevel, level))
        {
            var votingCategory = category.CreatePage("Voting", Color.white);

            // Vote kick
            if (serverSettings.VoteKickingEnabled.GetValue())
            {
                votingCategory.CreateFunction("Vote Kick", Color.red, () =>
                {
                    PlayerSender.SendVoteKickRequest(id);
                });
            }
        }

        // Create moderation options
        // If we are the server then we have full auth. Otherwise, check perm level
        if (!id.IsOwner && (NetworkInfo.IsServer || FusionPermissions.HasHigherPermissions(selfLevel, level)))
        {
            var moderationCategory = category.CreatePage("Moderation", Color.white);

            // Kick button
            if (FusionPermissions.HasSufficientPermissions(selfLevel, serverSettings.KickingAllowed.GetValue()))
            {
                moderationCategory.CreateFunction("Kick", Color.red, () =>
                {
                    PermissionSender.SendPermissionRequest(PermissionCommandType.KICK, id);
                });
            }

            // Ban button
            if (FusionPermissions.HasSufficientPermissions(selfLevel, serverSettings.BanningAllowed.GetValue()))
            {
                moderationCategory.CreateFunction("Ban", Color.red, () =>
                {
                    PermissionSender.SendPermissionRequest(PermissionCommandType.BAN, id);
                });
            }

            // Teleport buttons
            if (FusionPermissions.HasSufficientPermissions(selfLevel, serverSettings.Teleportation.GetValue()))
            {
                moderationCategory.CreateFunction("Teleport To Them", Color.red, () =>
                {
                    PermissionSender.SendPermissionRequest(PermissionCommandType.TELEPORT_TO_THEM, id);
                });

                moderationCategory.CreateFunction("Teleport To Us", Color.red, () =>
                {
                    PermissionSender.SendPermissionRequest(PermissionCommandType.TELEPORT_TO_US, id);
                });
            }
        }

        category.CreateFunction($"Platform ID: {longId}", Color.yellow, () =>
        {
            Clipboard.SetText(longId.ToString());
        });
        category.CreateFunction($"Instance ID: {smallId}", Color.yellow, () =>
        {
            Clipboard.SetText(smallId.ToString());
        });

        // Create VC options
        var voiceCategory = category.CreatePage("Voice Settings", Color.white);

        voiceCategory.CreateFloat("Volume", Color.white, startingValue: ContactsList.GetContact(id).volume, increment: 0.1f, 0f, 2f, (v) =>
        {
            var contact = ContactsList.GetContact(id);
            contact.volume = v;
            ContactsList.UpdateContact(contact);
        });
    }
}