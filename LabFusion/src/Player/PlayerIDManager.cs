namespace LabFusion.Player;

public static class PlayerIDManager
{
    public const int MaxNameLength = 32;

    public const int MinPlayerID = 0;
    public const int MaxPlayerID = byte.MaxValue;

    public static readonly HashSet<PlayerID> PlayerIDs = new();

    public static readonly Dictionary<byte, PlayerID> SmallIDLookup = new();
    public static readonly Dictionary<ulong, PlayerID> PlatformIDLookup = new();

    public static readonly HashSet<byte> ReservedSmallIDs = new();

    public static int PlayerCount => PlayerIDs.Count;
    public static bool HasOtherPlayers => PlayerCount > 1;

    public static ulong LocalPlatformID { get; private set; }
    public static byte LocalSmallID { get; private set; }
    public static PlayerID LocalID { get; private set; }

    public const byte HostSmallID = 0;

    public static void InsertPlayerID(PlayerID playerID)
    {
        if (SmallIDLookup.TryGetValue(playerID.SmallID, out var conflictingPlayer))
        {
            conflictingPlayer.Cleanup();
        }

        PlayerIDs.Add(playerID);
        SmallIDLookup[playerID.SmallID] = playerID;
        PlatformIDLookup[playerID.PlatformID] = playerID;

        ReserveSmallID(playerID.SmallID);
    }

    public static void RemovePlayerID(PlayerID playerID)
    {
        PlayerIDs.Remove(playerID);
        SmallIDLookup.Remove(playerID.SmallID);
        PlatformIDLookup.Remove(playerID.PlatformID);

        UnreserveSmallID(playerID.SmallID);
    }

    public static void ReserveSmallID(byte smallID)
    {
        ReservedSmallIDs.Add(smallID);
    }

    public static void UnreserveSmallID(byte smallID)
    {
        ReservedSmallIDs.Remove(smallID);
    }

    public static bool IsSmallIDReserved(byte smallID)
    {
        return ReservedSmallIDs.Contains(smallID);
    }

    public static byte? GetUniquePlayerID()
    {
        for (byte i = MinPlayerID; i < MaxPlayerID; i++)
        {
            if (!IsSmallIDReserved(i))
            {
                return i;
            }
        }

        return null;
    }

    public static PlayerID GetHostID()
    {
        return GetPlayerID(HostSmallID);
    }

    public static PlayerID GetPlayerID(byte smallID)
    {
        if (SmallIDLookup.TryGetValue(smallID, out var playerID))
        {
            return playerID;
        }

        return null;
    }

    public static PlayerID GetPlayerID(ulong platformID)
    {
        if (PlatformIDLookup.TryGetValue(platformID, out var playerID))
        {
            return playerID;
        }

        return null;
    }

    public static bool HasPlayerID(byte smallID) => SmallIDLookup.ContainsKey(smallID);

    public static bool HasPlayerID(ulong platformID) => PlatformIDLookup.ContainsKey(platformID);

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