using BoneLib.BoneMenu;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences.Server;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.Senders;

using UnityEngine;

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
        if (NetworkInfo.IsServer && !id.IsMe)
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

        category.CreateFunction($"Platform ID: {longId}", Color.yellow, () =>
        {
            GUIUtility.systemCopyBuffer = longId.ToString();
        });
        category.CreateFunction($"Instance ID: {smallId}", Color.yellow, () =>
        {
            GUIUtility.systemCopyBuffer = smallId.ToString();
        });

        // Create VC options
        var voiceCategory = category.CreatePage("Voice Settings", Color.white);

        voiceCategory.CreateFloat("Volume", Color.white, startingValue: ContactsList.GetContact(id).volume, increment: 0.1f, minValue: 0f, maxValue: 2f, callback: (v) =>
        {
            var contact = ContactsList.GetContact(id);
            contact.volume = v;
            ContactsList.UpdateContact(contact);
        });
    }
}