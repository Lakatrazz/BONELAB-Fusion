﻿using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;
using UnityEngine;

namespace LabFusion.BoneMenu
{
    internal static partial class BoneMenuCreator
    {
        public static void PopulateServerInfo(MenuCategory category)
        {
            CreatePlayerListMenu(category);
            CreateAdminActionsMenu(category);
            CreateServerInfoQuickAccess(category);
        }

        private static void CreateServerInfoQuickAccess(MenuCategory category)
        {
            var subPanel = category.CreateSubPanel("Quick Access", Color.yellow);
            subPanel.CreateFunctionElement("Server Settings", Color.white, () =>
            {
                MenuManager.SelectCategory(_serverSettingsCategory);
            });
        }
    }
}
