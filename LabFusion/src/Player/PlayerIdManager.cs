﻿using LabFusion.Preferences;

namespace LabFusion.Player;

public static class PlayerIdManager
{
    public const int MaxNameLength = 32;

    public const int MinPlayerId = 0;
    public const int MaxPlayerId = byte.MaxValue;

    public static readonly HashSet<PlayerId> PlayerIds = new();
    public static int PlayerCount => PlayerIds.Count;
    public static bool HasOtherPlayers => PlayerCount > 1;

    public static string LocalUsername { get; private set; } = "[unknown]";
    public static string LocalNickname => FusionPreferences.ClientSettings.Nickname.GetValue();

    public static ulong LocalLongId { get; private set; }
    public static byte LocalSmallId { get; private set; }
    public static PlayerId LocalId { get; private set; }

    public const byte HostSmallId = 0;

    public static byte? GetUnusedPlayerId()
    {
        for (byte i = 0; i < 255; i++)
        {
            if (GetPlayerId(i) == null)
                return i;
        }
        return null;
    }

    public static PlayerId GetHostId()
    {
        return GetPlayerId(HostSmallId);
    }

    public static PlayerId GetPlayerId(byte smallId)
    {
        return PlayerIds.FirstOrDefault(x => x.SmallId == smallId);
    }

    public static PlayerId GetPlayerId(ulong longId)
    {
        return PlayerIds.FirstOrDefault(x => x.LongId == longId);
    }

    public static bool HasPlayerId(byte smallId) => GetPlayerId(smallId) != null;

    public static bool HasPlayerId(ulong longId) => GetPlayerId(longId) != null;

    internal static void ApplyLocalId()
    {
        var id = GetPlayerId(LocalLongId);
        if (id != null)
        {
            LocalId = id;
            LocalSmallId = id.SmallId;
        }
        else
        {
            LocalId = null;
            LocalSmallId = 0;
        }
    }

    internal static void RemoveLocalId()
    {
        LocalId = null;
    }

    public static void SetLongId(ulong longId)
    {
        LocalLongId = longId;
    }

    public static void SetUsername(string username)
    {
        LocalUsername = username;
    }
}