#if DEBUG
using LabFusion.Network;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Debugging
{
    /// <summary>
    /// An empty networking layer for debugging purposes. This does not implement any multiplayer functionality.
    /// </summary>
    public class EmptyNetworkLayer : NetworkLayer
    {
        internal override void Disconnect(string reason = "") { }

        internal override void StartServer() { }

        internal override void OnCleanupLayer() { }

        internal override void OnInitializeLayer() {
            FusionLogger.Log("Initialized mod with an empty networking layer!", ConsoleColor.Magenta);
            FusionLogger.Log("This is for debugging purposes only, and will not allow multiplayer!", ConsoleColor.Magenta);
        }
    }
}
#endif