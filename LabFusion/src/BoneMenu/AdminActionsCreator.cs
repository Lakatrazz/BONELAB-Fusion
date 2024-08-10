using BoneLib.BoneMenu;

using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    public static partial class BoneMenuCreator
    {
        public static void CreateAdminActionsMenu(Page page)
        {
            // Root category
            var adminActions = page.CreatePage("Admin Actions", Color.white);
            adminActions.CreateFunction("Despawn All", Color.white, () =>
            {
                PooleeUtilities.DespawnAll();
            });
        }
    }
}
