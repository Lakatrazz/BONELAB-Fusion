using BoneLib.BoneMenu;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    using Menu = BoneLib.BoneMenu.Menu;

    public static partial class BoneMenuCreator
    {
        public static void PopulateServerInfo(Page page)
        {
            CreatePlayerListMenu(page);
            CreateAdminActionsMenu(page);
            CreateServerInfoQuickAccess(page);
        }

        private static void CreateServerInfoQuickAccess(Page page)
        {
            var subPanel = page.CreatePage("Quick Access", Color.yellow);
            subPanel.CreateFunction("Server Settings", Color.white, () =>
            {
                Menu.OpenPage(_serverSettingsCategory);
            });
        }
    }
}
