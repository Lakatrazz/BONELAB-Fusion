using LabFusion.Player;

namespace LabFusion.Network;

public static class NetworkAuthorityManager
{
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
