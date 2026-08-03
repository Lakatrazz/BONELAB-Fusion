using LabFusion.Network;

namespace LabFusion.Player;

public static class PlayerIDManager
{
    public const int MaxNameLength = 32;

    public const int MinPlayerID = 0;
    public const int MaxPlayerID = byte.MaxValue;

    public static readonly HashSet<PlayerID> PlayerIDs = new();

    public static readonly Dictionary<ClientSmallID, PlayerID> SmallIDLookup = new();
    public static readonly Dictionary<ClientPlatformID, PlayerID> PlatformIDLookup = new();

    public static readonly HashSet<ClientSmallID> ReservedSmallIDs = new();

    public static int PlayerCount => PlayerIDs.Count;
    public static bool HasOtherPlayers => PlayerCount > 1;

    public static ClientPlatformID LocalPlatformID { get; private set; }
    public static ClientSmallID LocalSmallID { get; private set; }
    public static PlayerID LocalID { get; private set; }

    public static readonly ClientSmallID HostSmallID = new(0);

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

    public static void ReserveSmallID(ClientSmallID smallID)
    {
        ReservedSmallIDs.Add(smallID);
    }

    public static void UnreserveSmallID(ClientSmallID smallID)
    {
        ReservedSmallIDs.Remove(smallID);
    }

    public static bool IsSmallIDReserved(ClientSmallID smallID)
    {
        return ReservedSmallIDs.Contains(smallID);
    }

    public static ClientSmallID? GetUniquePlayerID()
    {
        for (byte i = MinPlayerID; i < MaxPlayerID; i++)
        {
            var smallID = new ClientSmallID(i);

            if (!IsSmallIDReserved(smallID))
            {
                return smallID;
            }
        }

        return null;
    }

    public static PlayerID GetHostID()
    {
        return GetPlayerID(HostSmallID);
    }

    public static PlayerID GetPlayerID(ClientSmallID smallID)
    {
        if (SmallIDLookup.TryGetValue(smallID, out var playerID))
        {
            return playerID;
        }

        return null;
    }

    public static PlayerID GetPlayerID(ClientPlatformID platformID)
    {
        if (PlatformIDLookup.TryGetValue(platformID, out var playerID))
        {
            return playerID;
        }

        return null;
    }

    public static bool HasPlayerID(ClientSmallID smallID) => SmallIDLookup.ContainsKey(smallID);

    public static bool HasPlayerID(ClientPlatformID platformID) => PlatformIDLookup.ContainsKey(platformID);

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
            LocalSmallID = ClientSmallID.Empty;
        }
    }

    internal static void RemoveLocalID()
    {
        LocalID = null;
    }

    public static void SetPlatformID(ClientPlatformID platformID)
    {
        LocalPlatformID = platformID;
    }
}