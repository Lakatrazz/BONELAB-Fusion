using LabFusion.UI.Popups;
using LabFusion.Utilities;

using MelonLoader;

using System.Collections;

namespace LabFusion.Network.Proxy;

public sealed class ProxyMatchmaker : IMatchmaker
{
    private ProxyLobbyManager _lobbyManager = null;

    public ProxyMatchmaker(ProxyLobbyManager lobbyManager)
    {
        _lobbyManager = lobbyManager;
    }

    public void RequestLobbies(Action<IMatchmaker.MatchmakerCallbackInfo> callback) => RequestLobbies(MatchmakerFilters.Empty, callback);

    public void RequestLobbies(MatchmakerFilters filters, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        var version = FusionMod.Version;

        var parameters = new ProxyLobbyRequestParameters()
        {
            Filters = filters,
            VersionMajor = version.Major,
            VersionMinor = version.Minor,
            LobbyCode = null,
        };

        MelonCoroutines.Start(FindLobbies(parameters, callback));
    }

    public void RequestLobbiesByCode(string code, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        var version = FusionMod.Version;

        var parameters = new ProxyLobbyRequestParameters()
        {
            Filters = MatchmakerFilters.Empty,
            VersionMajor = version.Major,
            VersionMinor = version.Minor,
            LobbyCode = code,
        };

        MelonCoroutines.Start(FindLobbies(parameters, callback));
    }

    private static void SendTimeOutNotification()
    {
        Notifier.Send(new Notification()
        {
            Title = "Timed Out",
            Message = "Requesting Lobby IDs took too long.",
            ShowPopup = true,
            SaveToMenu = false,
        });
    }

    private IEnumerator FindLobbies(ProxyLobbyRequestParameters parameters, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        // Fetch lobbies
        var task = _lobbyManager.RequestLobbyIDs(parameters);

        float timeTaken = 0f;

        while (!task.IsCompleted)
        {
            yield return null;

            timeTaken += TimeUtilities.DeltaTime;

            if (timeTaken >= 20f)
            {
                SendTimeOutNotification();
                yield break;
            }
        }

        // Get metadata
        List<IMatchmaker.LobbyInfo> netLobbies = new();

        var lobbies = task.Result;

        foreach (var lobby in lobbies)
        {
            var metadataTask = _lobbyManager.RequestLobbyMetadataInfo(lobby);

            timeTaken = 0f;

            while (!metadataTask.IsCompleted)
            {
                yield return null;
                timeTaken += TimeUtilities.DeltaTime;

                if (timeTaken >= 20f)
                {
                    SendTimeOutNotification();
                    yield break;
                }
            }

            LobbyMetadataInfo metadata = metadataTask.Result;
            ProxyNetworkLobby networkLobby = new()
            {
                info = metadata,
            };

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
}