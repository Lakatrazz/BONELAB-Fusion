using BoneLib.BoneMenu.Elements;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    public static partial class BoneMenuCreator
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
