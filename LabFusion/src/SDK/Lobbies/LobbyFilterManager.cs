using LabFusion.Network;

namespace LabFusion.SDK.Lobbies;

public static class LobbyFilterManager
{
    private static readonly List<ILobbyFilter> _lobbyFilters = new();
    public static List<ILobbyFilter> LobbyFilters => _lobbyFilters;

    public static event Action<ILobbyFilter> OnAddedFilter;

    public static void LoadBuiltInFilters()
    {
        // Lobby length filter
        var lengthFilter = new GenericLobbyFilter("Hide Full Lobbies", (l, i) =>
        {
            return i.LobbyInfo.MaxPlayers > i.LobbyInfo.PlayerCount;
        });
        lengthFilter.SetActive(true);

        AddLobbyFilter(lengthFilter);

        // Outdated filter
        var outdatedFilter = new GenericLobbyFilter("Hide Mismatching Versions", (l, i) =>
        {
            return NetworkVerification.CompareVersion(i.LobbyInfo.LobbyVersion, FusionMod.Version) == VersionResult.Ok;
        });
        outdatedFilter.SetActive(true);

        AddLobbyFilter(outdatedFilter);

        // Friends filter
        var friendsFilter = new GenericLobbyFilter("Friends Only", (l, i) =>
        {
            return NetworkInfo.CurrentNetworkLayer.IsFriend(i.LobbyInfo.LobbyId);
        });

        AddLobbyFilter(friendsFilter);
    }

    public static void AddLobbyFilter(ILobbyFilter filter)
    {
        _lobbyFilters.Add(filter);

        OnAddedFilter?.Invoke(filter);
    }

    public static bool FilterLobby(INetworkLobby lobby, LobbyMetadataInfo info)
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
}