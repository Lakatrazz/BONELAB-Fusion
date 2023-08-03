using BoneLib.BoneMenu.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.BoneMenu
{
    internal static partial class BoneMenuCreator {
        public static void PopulateServerInfo(MenuCategory category) {
            CreatePlayerListMenu(category);
            CreateAdminActionsMenu(category);
        }
    }
}
