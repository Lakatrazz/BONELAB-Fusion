using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;

using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Senders;

using System;
using System.Windows.Forms;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    internal static partial class BoneMenuCreator {
        private static MenuCategory _playerListCategory;

        public static void CreatePlayerListMenu(MenuCategory category)
        {
            // Root category
            _playerListCategory = category.CreateCategory("Player List", Color.white);
            _playerListCategory.CreateFunctionElement("Refresh", Color.white, RefreshPlayerList);
            _playerListCategory.CreateFunctionElement("Select Refresh to load players!", Color.yellow, null);
        }

        private static void RefreshPlayerList() {
            // Clear existing lobbies
            _playerListCategory.Elements.Clear();
            _playerListCategory.CreateFunctionElement("Refresh", Color.white, RefreshPlayerList);

            // Add an item for every player
            foreach (var id in PlayerIdManager.PlayerIds) {
                CreatePlayer(id);
            }

            MenuManager.SelectCategory(_playerListCategory);
        }

        private static void CreatePlayer(PlayerId id) {
            // Get the name for the category
            string username = id.GetMetadata(MetadataHelper.UsernameKey);
            string nickname = id.GetMetadata(MetadataHelper.NicknameKey);

            string display;

            if (string.IsNullOrWhiteSpace(nickname))
                display = username;
            else
                display = $"{nickname} ({username})";

            // Get the current permission
            FusionPermissions.FetchPermissionLevel(id.LongId, out var level, out Color color);

            // Create the category and setup its options
            var category = _playerListCategory.CreateCategory(display, color);

            ulong longId = id.LongId;
            byte smallId = id.SmallId;

            // Set permission display
            if (NetworkInfo.IsServer && !id.IsSelf) {
                var permSetter = category.CreateEnumElement($"Permissions", Color.yellow, level, (v) => {
                    FusionPermissions.TrySetPermission(longId, username, v);
                });

                id.OnMetadataChanged += (player) => {
                    if (player.TryGetMetadata(MetadataHelper.PermissionKey, out string rawLevel) && Enum.TryParse(rawLevel, out PermissionLevel newLevel)) {
                        permSetter.SetValue(newLevel);
                    }
                };
            }
            else {
                var permDisplay = category.CreateFunctionElement($"Permissions: {level}", Color.yellow, null);

                id.OnMetadataChanged += (player) => {
                    if (player.TryGetMetadata(MetadataHelper.PermissionKey, out string rawLevel)) {
                        permDisplay.SetName($"Permissions: {rawLevel}");
                    }
                };
            }

            // Get self permissions
            FusionPermissions.FetchPermissionLevel(PlayerIdManager.LocalLongId, out var selfLevel, out _);

            // Create moderation options
            if (!id.IsSelf && FusionPermissions.HasHigherPermissions(selfLevel, level)) {
                var serverSettings = FusionPreferences.ActiveServerSettings;

                var moderationCategory = category.CreateCategory("Moderation", Color.white);

                // Kick button
                if (FusionPermissions.HasSufficientPermissions(selfLevel, serverSettings.KickingAllowed.GetValue())) {
                    moderationCategory.CreateFunctionElement("Kick", Color.red, () => {
                        PermissionSender.SendPermissionRequest(PermissionCommandType.KICK, id);
                    }, "Are you sure?");
                }

                // Ban button
                if (FusionPermissions.HasSufficientPermissions(selfLevel, serverSettings.BanningAllowed.GetValue())) {
                    moderationCategory.CreateFunctionElement("Ban", Color.red, () =>
                    {
                        PermissionSender.SendPermissionRequest(PermissionCommandType.BAN, id);
                    }, "Are you sure?");
                }

                // Teleport buttons
                if (FusionPermissions.HasSufficientPermissions(selfLevel, serverSettings.Teleportation.GetValue()))
                {
                    moderationCategory.CreateFunctionElement("Teleport To Them", Color.red, () =>
                    {
                        PermissionSender.SendPermissionRequest(PermissionCommandType.TELEPORT_TO_THEM, id);
                    }, "Are you sure?");

                    moderationCategory.CreateFunctionElement("Teleport To Us", Color.red, () =>
                    {
                        PermissionSender.SendPermissionRequest(PermissionCommandType.TELEPORT_TO_US, id);
                    }, "Are you sure?");
                }
            }

            category.CreateFunctionElement($"Platform ID: {longId}", Color.yellow, () => {
                Clipboard.SetText(longId.ToString());
            });
            category.CreateFunctionElement($"Instance ID: {smallId}", Color.yellow, () => {
                Clipboard.SetText(smallId.ToString());
            });
        }
    }
}
