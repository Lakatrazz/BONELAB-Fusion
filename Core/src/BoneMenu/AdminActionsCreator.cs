using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;
using System;
using System.Windows.Forms;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    internal static partial class BoneMenuCreator
    {
        public static void CreateAdminActionsMenu(MenuCategory category)
        {
            // Root category
            var adminActions = category.CreateCategory("Admin Actions", Color.white);
            adminActions.CreateFunctionElement("Despawn All", Color.white, () =>
            {
                PooleeUtilities.DespawnAll();
            }, "Are you sure?");
        }
    }
}
