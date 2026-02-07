using LabFusion.Player;
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
        {
            return VersionResult.Lower;
        }
        else if (server > user)
        {
            return VersionResult.Higher;
        }

        return VersionResult.Ok;
    }

    /// <summary>
    /// Returns true if the client is approved to join this server.
    /// </summary>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static bool IsClientApproved(string platformID)
    {
        var privacy = SavedServerSettings.Privacy.Value;

        return privacy switch
        {
            ServerPrivacy.PUBLIC or ServerPrivacy.PRIVATE => true,
            ServerPrivacy.FRIENDS_ONLY => NetworkHelper.IsFriend(platformID),
            _ => false,
        };
    }

    /// <summary>
    /// Checks if a sender has authority to affect a player. 
    /// Authority is given when the player and the sender are the same or the sender is the host.
    /// If the sender is null, then authority is not given.
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="senderID"></param>
    /// <returns></returns>
    public static bool HasAuthorityOverPlayer(byte playerID, byte? senderID)
    {
        // Must have a sender to have authority
        if (!senderID.HasValue)
        {
            return false;
        }

        var senderValue = senderID.Value;

        bool senderIsHost = senderValue == PlayerIDManager.HostSmallID;
        bool senderIsPlayer = senderValue == playerID;

        return senderIsHost || senderIsPlayer;
    }
}