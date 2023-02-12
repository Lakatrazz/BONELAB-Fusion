using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;

using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;

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
            PlayerPermissions.FetchPermissionLevel(id.LongId, out var level, out Color color);

            // Create the category and setup its options
            var category = _playerListCategory.CreateCategory(display, color);

            ulong longId = id.LongId;
            byte smallId = id.SmallId;

            // Set permission display
            if (NetworkInfo.IsServer && !id.IsSelf) {
                var permSetter = category.CreateEnumElement($"Permissions", Color.yellow, level, (v) => {
                    PlayerPermissions.TrySetPermission(longId, username, v);
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

            // Create moderation options
            // Note: replace IsServer with permission check
            if (NetworkInfo.IsServer && !id.IsSelf) {
                var moderationCategory = category.CreateCategory("Moderation", Color.white);
                moderationCategory.CreateFunctionElement("Kick", Color.red, () => {
                    NetworkHelper.KickUser(id);
                }, "Are you sure?");

                moderationCategory.CreateFunctionElement("Ban", Color.red, () => {
                    NetworkHelper.BanUser(id);
                }, "Are you sure?");
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
