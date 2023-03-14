using BoneLib.BoneMenu.Elements;
using LabFusion.Network;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Network
{
    /// <summary>
    /// An empty networking layer for fallback. This does not implement any multiplayer functionality.
    /// </summary>
    public class EmptyNetworkLayer : NetworkLayer
    {
        internal override void Disconnect(string reason = "") { }

        internal override void StartServer() { }

        internal override void OnCleanupLayer() { }

        internal override void OnInitializeLayer() {
            FusionLogger.Log("Initialized mod with an empty networking layer!", ConsoleColor.Magenta);
#if DEBUG
            FusionLogger.Log("This is for debugging purposes only, and will not allow multiplayer!", ConsoleColor.Magenta);
#else
            FusionLogger.Log("This usually means all other network layers failed to initialize, or you selected Empty in the settings.", ConsoleColor.Magenta);
#endif
        }

        internal override void OnSetupBoneMenu(MenuCategory category) {
            base.OnSetupBoneMenu(category);

            // Info for people incase this layer ends up being selected
            category.CreateFunctionElement("You currently have no networking selected.", Color.white, null);
            category.CreateFunctionElement("This means you likely do not have Steam open.", Color.white, null);
            category.CreateFunctionElement("Please install and open Steam.", Color.white, null);
        }
    }
}