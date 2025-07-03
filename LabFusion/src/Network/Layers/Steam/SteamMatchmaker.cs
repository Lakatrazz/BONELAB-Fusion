using MelonLoader;

using Steamworks;
using Steamworks.Data;

using System.Collections;

namespace LabFusion.Network;

public sealed class SteamMatchmaker : IMatchmaker
{
    private delegate Task<Lobby[]> LobbySearchDelegate();

    public void RequestLobbies(Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        MelonCoroutines.Start(FindLobbies(FetchLobbies, callback));
    }

    public void RequestLobbiesByCode(string code, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        MelonCoroutines.Start(FindLobbies(FetchLobbies, callback));

        Task<Lobby[]> FetchLobbies() => FetchLobbiesByCode(code);
    }

    private static IEnumerator FindLobbies(LobbySearchDelegate searchDelegate, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        // Fetch lobbies
        var task = searchDelegate();

        while (!task.IsCompleted)
        {
            yield return null;
        }

        var lobbies = task.Result;

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

    private static Task<Lobby[]> FetchLobbies()
    {
        return SteamMatchmaking.LobbyList
            .FilterDistanceWorldwide()
            .WithKeyValue(LobbyKeys.HasServerOpenKey, bool.TrueString)
            .WithNotEqual(LobbyKeys.PrivacyKey, (int)ServerPrivacy.PRIVATE)
            .WithNotEqual(LobbyKeys.PrivacyKey, (int)ServerPrivacy.LOCKED)
            .RequestAsync();
    }

    private static Task<Lobby[]> FetchLobbiesByCode(string code)
    {
        return SteamMatchmaking.LobbyList
            .FilterDistanceWorldwide()
            .WithKeyValue(LobbyKeys.HasServerOpenKey, bool.TrueString)
            .WithKeyValue(LobbyKeys.LobbyCodeKey, code)
            .RequestAsync();
    }
}