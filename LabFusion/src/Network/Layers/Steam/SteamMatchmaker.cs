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

        List<IMatchmaker.LobbyInfo> netLobbies = new();

        foreach (var lobby in lobbies)
        {
            // Make sure this is not us
            if (lobby.Owner.IsMe)
            {
                continue;
            }

            var networkLobby = new SteamLobby(lobby);
            var metadata = LobbyMetadataHelper.ReadInfo(networkLobby);

            if (!metadata.HasServerOpen)
            {
                continue;
            }

            netLobbies.Add(new IMatchmaker.LobbyInfo()
            {
                lobby = networkLobby,
                metadata = metadata,
            });
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