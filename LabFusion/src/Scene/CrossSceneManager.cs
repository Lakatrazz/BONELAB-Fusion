using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Scene;

public static class CrossSceneManager
{
    // In the future, have fake hosts per scene to control events
    // These will be determined by the first person to load in if cross scene is enabled
    // But for now, just use the global host
    public static bool IsSceneHost()
    {
        return NetworkInfo.IsServer;
    }

    public static bool InCurrentScene(PlayerId player)
    {
        return true;
    }

    public static bool InScene(PlayerId player, string barcode)
    {
        return true;
    }
}
