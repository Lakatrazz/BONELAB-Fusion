namespace LabFusion.Player;

public static class PlayerIDManager
{
    public const int MaxNameLength = 32;

    public const int MinPlayerId = 0;
    public const int MaxPlayerId = byte.MaxValue;

    public static readonly HashSet<PlayerID> PlayerIds = new();
    public static int PlayerCount => PlayerIds.Count;
    public static bool HasOtherPlayers => PlayerCount > 1;

    public static ulong LocalPlatformID { get; private set; }
    public static byte LocalSmallID { get; private set; }
    public static PlayerID LocalID { get; private set; }

    public const byte HostSmallID = 0;

    public static byte? GetUnusedPlayerID()
    {
        for (byte i = 0; i < 255; i++)
        {
            if (GetPlayerID(i) == null)
                return i;
        }
        return null;
    }

    public static PlayerID GetHostID()
    {
        return GetPlayerID(HostSmallID);
    }

    public static PlayerID GetPlayerID(byte smallId)
    {
        return PlayerIds.FirstOrDefault(x => x.SmallID == smallId);
    }

    public static PlayerID GetPlayerID(ulong longId)
    {
        return PlayerIds.FirstOrDefault(x => x.PlatformID == longId);
    }

    public static bool HasPlayerID(byte smallId) => GetPlayerID(smallId) != null;

    public static bool HasPlayerID(ulong longId) => GetPlayerID(longId) != null;

    internal static void ApplyLocalID()
    {
        var id = GetPlayerID(LocalPlatformID);
        if (id != null)
        {
            LocalID = id;
            LocalSmallID = id.SmallID;
        }
        else
        {
            LocalID = null;
            LocalSmallID = 0;
        }
    }

    internal static void RemoveLocalID()
    {
        LocalID = null;
    }

    public static void SetLongID(ulong longID)
    {
        LocalPlatformID = longID;
    }
}