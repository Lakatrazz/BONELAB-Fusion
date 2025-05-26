using LabFusion.Preferences.Client;
using LabFusion.Player;

namespace LabFusion.Network;

/// <summary>
/// Internal class used for creating network layers and updating them.
/// </summary>
public static class InternalLayerHelpers
{
    public static void OnUpdateLayer()
    {
        NetworkLayerManager.Layer?.OnUpdateLayer();
    }

    public static void OnLateUpdateLayer()
    {
        NetworkLayerManager.Layer?.OnLateUpdateLayer();
    }

    public static void OnUserJoin(PlayerID id)
    {
        NetworkLayerManager.Layer?.OnUserJoin(id);
    }
}
