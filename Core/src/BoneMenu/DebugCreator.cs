#if DEBUG
using BoneLib.BoneMenu.Elements;

using LabFusion.Data;
using LabFusion.Representation;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    internal static partial class BoneMenuCreator
    {
        public static void CreateDebugMenu(MenuCategory category)
        {
            var debugCategory = category.CreateCategory("DEBUG", Color.red);
            debugCategory.CreateFunctionElement("Spawn Player Rep", Color.white, () =>
            {
                PlayerRepUtilities.CreateNewRig((rig) =>
                {
                    rig.transform.position = RigData.RigReferences.RigManager.physicsRig.feet.transform.position;
                });
            });
        }
    }
}
#endif