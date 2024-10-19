using MelonLoader;

using Steamworks;
using Steamworks.Data;

using System.Collections;

namespace LabFusion.Network;

public sealed class SteamMatchmaker : IMatchmaker
{
    public void RequestLobbies(Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        MelonCoroutines.Start(FindLobbies(callback));
    }

    private static IEnumerator FindLobbies(Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        // Fetch lobbies
        var task = FetchLobbies();

        while (!task.IsCompleted)
        {
            yield return null;
        }

        var lobbies = task.Result;

        List<INetworkLobby> netLobbies = new();

        foreach (var lobby in lobbies)
        {
            // Make sure this is not us
            if (lobby.Owner.IsMe)
            {
                continue;
            }

            var networkLobby = new SteamLobby(lobby);

            netLobbies.Add(networkLobby);
        }

        var info = new IMatchmaker.MatchmakerCallbackInfo()
        {
            lobbies = netLobbies.ToArray(),
        };

        callback?.Invoke(info);
    }

    private static Task<Lobby[]> FetchLobbies()
    {
        var list = SteamMatchmaking.LobbyList;
        list.FilterDistanceWorldwide();
        list.WithMaxResults(int.MaxValue);
        list.WithSlotsAvailable(int.MaxValue);
        list.WithKeyValue(LobbyConstants.HasServerOpenKey, bool.TrueString);
        return list.RequestAsync();
    }
}