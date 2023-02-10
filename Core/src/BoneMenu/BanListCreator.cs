using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;

using LabFusion.Data;
using LabFusion.Network;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    internal static partial class BoneMenuCreator {
        private static MenuCategory _banListCategory;

        public static void CreateBanListMenu(MenuCategory category)
        {
            // Root category
            _banListCategory = category.CreateCategory("Banned Players", Color.red);
            _banListCategory.CreateFunctionElement("Refresh", Color.white, RefreshBanList);
            _banListCategory.CreateFunctionElement("Select Refresh to load banned players!", Color.yellow, null);
        }

        private static void RefreshBanList() {
            // Clear existing lobbies
            _banListCategory.Elements.Clear();
            _banListCategory.CreateFunctionElement("Refresh", Color.white, RefreshBanList);

            // Pull the latest file info
            BanList.PullFromFile();

            // Add a ban item for every banned player
            foreach (var tuple in BanList.BannedUsers) {
                CreateBannedPlayer(tuple.Item1, tuple.Item2, tuple.Item3);
            }

            MenuManager.SelectCategory(_banListCategory);
        }

        private static void CreateBannedPlayer(ulong longId, string username, string reason) {
            var category = _banListCategory.CreateCategory($"{username}", Color.white);
            category.CreateFunctionElement($"Username: {username}", Color.yellow, null);
            category.CreateFunctionElement($"Platform ID: {longId}", Color.yellow, null);
            category.CreateFunctionElement($"Ban Reason: {reason}", Color.yellow, null);
            category.CreateFunctionElement($"Pardon", Color.red, () => {
                NetworkHelper.PardonUser(longId);
                RefreshBanList();
            }, "Are you sure?");
        }
    }
}
