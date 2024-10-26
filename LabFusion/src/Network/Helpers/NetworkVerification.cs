using LabFusion.Preferences.Server;

namespace LabFusion.Network;

public enum VersionResult
{
    Unknown = 0,
    Ok = 1,
    Lower = 2,
    Higher = 3,
}

/// <summary>
/// Helper class for verifying users and actions.
/// </summary>
public static class NetworkVerification
{
    /// <summary>
    /// Compares the server and user versions.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static VersionResult CompareVersion(Version server, Version user)
    {
        // We don't care about the patch/build number
        server = new Version(server.Major, server.Minor, 0);
        user = new Version(user.Major, user.Minor, 0);

        if (server < user)
            return VersionResult.Lower;
        else if (server > user)
            return VersionResult.Higher;

        return VersionResult.Ok;
    }

    /// <summary>
    /// Returns true if the client is approved to join this server.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static bool IsClientApproved(ulong userId)
    {
        var privacy = SavedServerSettings.Privacy.Value;

        switch (privacy)
        {
            default:
            case ServerPrivacy.LOCKED:
                return false;
            case ServerPrivacy.PUBLIC:
            case ServerPrivacy.PRIVATE:
                return true;
            case ServerPrivacy.FRIENDS_ONLY:
                return NetworkHelper.IsFriend(userId);
        }
    }
}