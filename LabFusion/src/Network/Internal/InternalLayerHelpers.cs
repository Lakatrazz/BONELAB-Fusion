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
}
