using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;

using LabFusion.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities {
    internal static class FusionPreferences {
        internal static MenuCategory fusionCategory;

        internal static void OnInitializePreferences() {
            fusionCategory = MenuManager.CreateCategory("BONELAB Fusion", Color.white);

            InternalLayerHelpers.OnSetupBoneMenuLayer(fusionCategory);
        }
    }
}
