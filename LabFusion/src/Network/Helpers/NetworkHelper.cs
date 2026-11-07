namespace LabFusion.Network;

/// <summary>
/// Helper class for calling basic methods on the Server or Client.
/// </summary>
public static class NetworkHelper
{
    /// <summary>
    /// Returns true if this user is friended on the active network platform.
    /// </summary>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static bool IsFriend(ClientPlatformID platformID)
    {
        if (NetworkLayerManager.Layer != null)
            return NetworkLayerManager.Layer.IsFriend(platformID);

        return false;
    }
}