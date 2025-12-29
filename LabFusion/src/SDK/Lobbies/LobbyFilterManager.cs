using LabFusion.Network;
using LabFusion.Safety;

namespace LabFusion.SDK.Lobbies;

public static class LobbyFilterManager
{
    private static readonly List<ILobbyFilter> _lobbyFilters = new();
    public static List<ILobbyFilter> LobbyFilters => _lobbyFilters;

    public static event Action<ILobbyFilter> OnAddedFilter;

    public static GenericLobbyFilter FullFilter { get; } = new("Hide Full Lobbies", (l, i) =>
    {
        return i.LobbyInfo.MaxPlayers > i.LobbyInfo.PlayerCount;
    });

    public static GenericLobbyFilter MismatchingVersionsFilter { get; } = new("Hide Mismatching Versions", (l, i) =>
    {
        return NetworkVerification.CompareVersion(i.LobbyInfo.LobbyVersion, FusionMod.Version) == VersionResult.Ok;
    });

    public static GenericLobbyFilter FriendsFilter { get; } = new("Friends Only", (l, i) =>
    {
        return NetworkLayerManager.Layer.IsFriend(i.LobbyInfo.LobbyID);
    });

    public static void LoadBuiltInFilters()
    {
        FullFilter.SetActive(true);
        AddLobbyFilter(FullFilter);

        MismatchingVersionsFilter.SetActive(true);
        AddLobbyFilter(MismatchingVersionsFilter);

        AddLobbyFilter(FriendsFilter);
    }

    public static void AddLobbyFilter(ILobbyFilter filter)
    {
        _lobbyFilters.Add(filter);

        OnAddedFilter?.Invoke(filter);
    }

    public static bool CheckOptionalFilters(INetworkLobby lobby, LobbyMetadataInfo info)
    {
        foreach (var filter in LobbyFilters)
        {
            if (filter.IsActive() && !filter.FilterLobby(lobby, info))
            {
                return false;
            }
        }

        return true;
    }

    public static bool CheckPersistentFilters(INetworkLobby lobby, LobbyMetadataInfo info)
    {
        if (GlobalBanManager.IsBanned(info.LobbyInfo))
        {
            return false;
        }

        return true;
    }

    public static MatchmakerFilters CreateMatchmakerFilters()
    {
        return new MatchmakerFilters()
        {
            FilterFull = FullFilter.IsActive(),
            FilterMismatchingVersions = MismatchingVersionsFilter.IsActive(),
        };
    }
}