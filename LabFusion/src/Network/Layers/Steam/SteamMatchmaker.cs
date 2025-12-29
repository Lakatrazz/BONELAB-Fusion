using LabFusion.Utilities;

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
        MelonCoroutines.Start(FindLobbies(FetchLobbies, filters, callback));
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

            if (!metadata.HasServerOpen)
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
            .WithKeyValue(LobbyKeys.HasServerOpenKey, bool.TrueString)
            .WithKeyValue(LobbyKeys.GameKey, GameHelper.GameName);
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