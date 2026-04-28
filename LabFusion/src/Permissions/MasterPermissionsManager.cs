using LabFusion.Safety;

namespace LabFusion.Permissions;

/// <summary>
/// The master permissions for a trusted user.
/// </summary>
public enum MasterStatus
{
    /// <summary>
    /// This is a regular user. No special permissions.
    /// </summary>
    None,

    /// <summary>
    /// This is a master user. Cannot be banned or kicked from lobbies.
    /// </summary>
    Master,
}

public static class MasterPermissionsManager
{
    /// <summary>
    /// Returns whether a user is on the master list.
    /// </summary>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static bool IsMaster(string platformID)
    {
        return TrustedListManager.VerifyPlayer(platformID, string.Empty) == TrustedStatus.Master;
    }

    /// <summary>
    /// Gets the master status for a given user.
    /// </summary>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static MasterStatus GetMasterStatus(string platformID)
    {
        if (IsMaster(platformID))
        {
            return MasterStatus.Master;
        }
        else
        {
            return MasterStatus.None;
        }
    }
}
