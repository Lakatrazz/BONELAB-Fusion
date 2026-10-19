using LabFusion.Utilities;
using LabFusion.Support;

using MelonLoader;

using Steamworks;
using Steamworks.Data;

using System.Collections;

namespace LabFusion.Network;

public sealed class SteamMatchmaker : IMatchmaker
{
    private delegate Task<Lobby[]> LobbySearchDelegate(MatchmakerFilters filters);

    public void RequestLobbies(Action<IMatchmaker.MatchmakerCallbackInfo> callback) => RequestLobbies(MatchmakerFilters.Empty, callback);

    public void RequestLobbies(MatchmakerFilters filters, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        MelonCoroutines.Start(FindLobbies(FetchLobbiesWithFriends, filters, callback));
    }

    public void RequestLobbiesByCode(string code, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        MelonCoroutines.Start(FindLobbies(FetchLobbies, MatchmakerFilters.Empty, callback));

        Task<Lobby[]> FetchLobbies(MatchmakerFilters filters) => FetchLobbiesByCode(code);
    }

    private static IEnumerator FindLobbies(LobbySearchDelegate searchDelegate, MatchmakerFilters filters, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        // Fetch lobbies
        var task = searchDelegate(filters);

        // Wait for the lobby search to complete
        while (!task.IsCompleted)
        {
            yield return null;
        }

        // If the lobby search errored, return an empty list and log the reason why
        if (!task.IsCompletedSuccessfully)
        {
            FusionLogger.LogException("searching for lobbies", task.Exception);
            callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
            yield break;
        }

        var lobbies = task.Result;

        // Steam can return null if none are available
        if (lobbies == null)
        {
            callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
            yield break;
        }

        List<IMatchmaker.LobbyInfo> netLobbies = new();

        foreach (var lobby in lobbies)
        {
            // Make sure this is not us
            if (lobby.Owner.IsMe)
            {
                continue;
            }

            var networkLobby = new SteamLobby(lobby);
            var metadata = LobbyMetadataSerializer.ReadInfo(networkLobby);

            if (!metadata.HasLobbyOpen)
            {
                continue;
            }

            netLobbies.Add(new IMatchmaker.LobbyInfo()
            {
                Lobby = networkLobby,
                Metadata = metadata,
            });
        }

        var info = new IMatchmaker.MatchmakerCallbackInfo()
        {
            Lobbies = netLobbies.ToArray(),
        };

        callback?.Invoke(info);
    }

    private static Task<Lobby[]> FetchLobbies(MatchmakerFilters filters)
    {
        var query = SteamMatchmaking.LobbyList;
        query = AddPersistentFilters(query);
        query = AddMatchmakingFilters(query, filters);

        return query
            .WithNotEqual(LobbyKeys.PrivacyKey, (int)ServerPrivacy.PRIVATE)
            .WithNotEqual(LobbyKeys.PrivacyKey, (int)ServerPrivacy.LOCKED)
            .RequestAsync();
    }

    private static async Task<Lobby[]> FetchLobbiesWithFriends(MatchmakerFilters filters)
    {
        var publicTask = FetchLobbies(filters);

        if (!filters.IncludeFriendPrivateLobbies)
        {
            return await publicTask;
        }

        var friendTask = FetchFriendPrivateLobbies(filters);

        var publicLobbies = await publicTask;

        // A failed friend search should never hide the regular results
        Lobby[] friendLobbies = null;

        try
        {
            friendLobbies = await friendTask;
        }
        catch (Exception e)
        {
            FusionLogger.LogException("searching for friend lobbies", e);
        }

        if (friendLobbies == null)
        {
            return publicLobbies;
        }

        var merged = new List<Lobby>(publicLobbies ?? Array.Empty<Lobby>());

        foreach (var lobby in friendLobbies)
        {
            if (!merged.Any(l => l.Id == lobby.Id))
            {
                merged.Add(lobby);
            }
        }

        return merged.ToArray();
    }

    /// <summary>
    /// Finds private lobbies whose host is a Steam friend and has explicitly allowed friends to join without a code.
    /// </summary>
    private static async Task<Lobby[]> FetchFriendPrivateLobbies(MatchmakerFilters filters)
    {
        var query = SteamMatchmaking.LobbyList;
        query = AddPersistentFilters(query);
        query = AddMatchmakingFilters(query, filters);

        var lobbies = await query
            .WithEqual(LobbyKeys.PrivacyKey, (int)ServerPrivacy.PRIVATE)
            .WithKeyValue(LobbyKeys.FriendsCanJoinKey, bool.TrueString)
            .RequestAsync();

        // Steam can't filter by owner, so only keep lobbies hosted by friends
        return lobbies?.Where(l => l.Owner.IsFriend).ToArray();
    }

    private static Task<Lobby[]> FetchLobbiesByCode(string code)
    {
        var query = SteamMatchmaking.LobbyList;
        query = AddPersistentFilters(query);

        return query
            .WithKeyValue(LobbyKeys.LobbyCodeKey, code.ToUpper())
            .RequestAsync();
    }

    private static LobbyQuery AddPersistentFilters(LobbyQuery query)
    {
        return query
            .FilterDistanceWorldwide()
            .WithKeyValue(LobbyKeys.IdentifierKey, bool.TrueString)
            .WithKeyValue(LobbyKeys.HasLobbyOpenKey, bool.TrueString)
            .WithKeyValue(LobbyKeys.GameKey, GameInfo.GameName);
    }

    private static LobbyQuery AddMatchmakingFilters(LobbyQuery query, MatchmakerFilters filters)
    {
        if (filters.FilterFull)
        {
            query = query.WithKeyValue(LobbyKeys.FullKey, bool.FalseString);
        }

        if (filters.FilterMismatchingVersions)
        {
            var version = FusionMod.Version;
            var versionMajor = version.Major;
            var versionMinor = version.Minor;

            query = query
                .WithEqual(LobbyKeys.VersionMajorKey, versionMajor)
                .WithEqual(LobbyKeys.VersionMinorKey, versionMinor);
        }

        return query;
    }
}