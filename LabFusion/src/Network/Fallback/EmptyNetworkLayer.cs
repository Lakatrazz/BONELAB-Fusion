using LabFusion.Utilities;

namespace LabFusion.Network
{
    /// <summary>
    /// An empty networking layer for fallback. This does not implement any multiplayer functionality.
    /// </summary>
    public class EmptyNetworkLayer : NetworkLayer
    {
        public override string Title => "Empty";

        public override void Disconnect(string reason = "") { }

        public override void StartServer() { }

        public override bool CheckSupported()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        public override bool CheckValidation()
        {
            return true;
        }

        public override void OnInitializeLayer()
        {
            FusionLogger.Log("Initialized mod with an empty networking layer!", ConsoleColor.Magenta);
#if DEBUG
            FusionLogger.Log("This is for debugging purposes only, and will not allow multiplayer!", ConsoleColor.Magenta);
#else
            FusionLogger.Log("This usually means all other network layers failed to initialize, or you selected Empty in the settings.", ConsoleColor.Magenta);
#endif
        }

        public override void OnCleanupLayer() 
        {
        }
    }
}