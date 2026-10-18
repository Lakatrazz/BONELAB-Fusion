using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using LabFusion.Network;
using LabFusion.Utilities;

namespace MarrowFusion.Epic;

internal class EpicMatchmaker : Matchmaker
{
    internal EOSRuntime Runtime;
    
    internal EpicMatchmaker(EOSRuntime runtime)
    {
        Runtime = runtime;
    }
    
    public override ILobbyQuery CreateQuery()
    {
        var searchOptions = new CreateLobbySearchOptions()
        {
            MaxResults = 200,
        };

        var result = Runtime.Lobby.LobbyInterface.CreateLobbySearch(ref searchOptions, out var lobbySearch);
        if (result != Result.Success || lobbySearch == null)
        {
            EpicModule.Logger.Error($"Failed to create lobby search: {result}");
            return null;
        }

        return new EpicLobbyQuery(lobbySearch);
    }
    
    protected override async Task<MatchmakerResult> TrySearchLobbiesAsync(ILobbyQuery query, CancellationToken cancellationToken)
    {
        if (query is not EpicLobbyQuery epicLobbyQuery)
        {
            throw new ArgumentException(null, nameof(query));
        }

        var lobbySearch = epicLobbyQuery.LobbySearch;

        var tcs = new TaskCompletionSource<Result>();

        var findOptions = new LobbySearchFindOptions()
        {
            LocalUserId = Runtime.Connect.LocalUserId,
        };

        await ThreadHelper.RunOnMainThreadAsTask(() =>
        {
            lobbySearch.Find(ref findOptions, null, (ref LobbySearchFindCallbackInfo info) =>
            {
                tcs.TrySetResult(info.ResultCode);
            });
        });

        await using (cancellationToken.Register(() => tcs.TrySetCanceled()))
        {
            var findResult = await tcs.Task;

            if (findResult != Result.Success)
            {
                return MatchmakerResult.Empty;
            }
        }
        
        var resultSegment = await ThreadHelper.RunOnMainThreadAsTask(() =>
        {
            var countOptions = new LobbySearchGetSearchResultCountOptions();
            
            uint resultCount = lobbySearch.GetSearchResultCount(ref countOptions);

            if (resultCount <= 0)
            {
                return new ArraySegment<LobbyMetadata>(Array.Empty<LobbyMetadata>());
            }

            var results = new LobbyMetadata[resultCount];
            int validCount = 0;

            for (uint i = 0; i < resultCount; i++)
            {
                try
                {
                    var copyOptions = new LobbySearchCopySearchResultByIndexOptions()
                    {
                        LobbyIndex = i,
                    };

                    var copyResult = lobbySearch.CopySearchResultByIndex(ref copyOptions, out var lobbyDetails);
                    if (copyResult != Result.Success || lobbyDetails == null)
                    {
                        continue;
                    }

                    var getLobbyOwnerOptions = new LobbyDetailsGetLobbyOwnerOptions();
                    
                    var lobbyOwner = lobbyDetails.GetLobbyOwner(ref getLobbyOwnerOptions);
                    
                    if (lobbyOwner == null || !lobbyOwner.IsValid())
                    {
                        lobbyDetails.Release();
                        continue;
                    }
                    
                    if (lobbyOwner == Runtime.Connect.LocalUserId)
                    {
                        lobbyDetails.Release();
                        continue;
                    }

                    using var networkLobby = new EpicLobby(Runtime, lobbyDetails, lobbyOwner);

                    if (!LobbyMetadata.TryReadFromLobby(networkLobby, out var metadata))
                    {
                        lobbyDetails.Release();
                        continue;
                    }

                    results[validCount++] = metadata;
                }
                catch (Exception ex)
                {
                    EpicModule.Logger.LogException("parsing lobby result", ex);
                }
            }

            return new ArraySegment<LobbyMetadata>(results, 0, validCount);
        });

        return new MatchmakerResult(resultSegment);
    }
}