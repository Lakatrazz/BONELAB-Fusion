using BoneLib.BoneMenu;

using LabFusion.Data;
using LabFusion.Network;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    using Menu = BoneLib.BoneMenu.Menu;

    public static partial class BoneMenuCreator
    {
        private static Page _banListCategory;

        public static void CreateBanListMenu(Page page)
        {
            // Root category
            _banListCategory = page.CreatePage("Banned Players", Color.red);
            _banListCategory.CreateFunction("Refresh", Color.white, RefreshBanList);
            _banListCategory.CreateFunction("Select Refresh to load banned players!", Color.yellow, null);
        }

        private static void RefreshBanList()
        {
            // Clear existing lobbies
            _banListCategory.RemoveAll();
            _banListCategory.CreateFunction("Refresh", Color.white, RefreshBanList);

            // Pull the latest file info
            BanManager.ReadFile();

            // Add a ban item for every banned player
            //foreach (var tuple in BanManager.BannedUsers)
            //{
            //    CreateBannedPlayer(tuple.Item1, tuple.Item2, tuple.Item3);
            //}

            Menu.OpenPage(_banListCategory);
        }

        private static void CreateBannedPlayer(ulong longId, string username, string reason)
        {
            var category = _banListCategory.CreatePage($"{username}", Color.white);
            category.CreateFunction($"Username: {username}", Color.yellow, null);
            category.CreateFunction($"Platform ID: {longId}", Color.yellow, null);
            category.CreateFunction($"Ban Reason: {reason}", Color.yellow, null);
            category.CreateFunction($"Pardon", Color.red, () =>
            {
                NetworkHelper.PardonUser(longId);
                RefreshBanList();
            });
        }
    }
}
