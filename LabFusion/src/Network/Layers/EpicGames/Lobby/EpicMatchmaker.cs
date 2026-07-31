using System.Collections;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using LabFusion.Support;
using LabFusion.Utilities;
using MelonLoader;

namespace LabFusion.Network;

internal class EpicMatchmaker : IMatchmaker
{
    private const int DefaultMaxResults = 200;
    private const int CodeSearchMaxResults = 1;
    private const int SearchTimeoutSeconds = 30;
    
    internal EOSRuntime Runtime;
    internal ProductUserId LocalUserId;
    
    internal EpicMatchmaker(EOSRuntime runtime, ProductUserId localUserId)
    {
        Runtime = runtime;
        LocalUserId = localUserId;
    }

    public void RequestLobbies(Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        RequestLobbies(MatchmakerFilters.Empty, callback);
    }

    public void RequestLobbies(MatchmakerFilters filters, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        RequestLobbies(DefaultMaxResults, null, filters, callback);
    }

    public void RequestLobbiesByCode(string code, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        RequestLobbies(CodeSearchMaxResults, code, MatchmakerFilters.Empty, callback);
    }

    private void RequestLobbies(int maxResults, string code, MatchmakerFilters filters, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        MelonCoroutines.Start(FindLobbies(maxResults, code, filters, callback));
    }

    private IEnumerator FindLobbies(int maxResults, string code, MatchmakerFilters filters, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        var createLobbySearchOptions = new CreateLobbySearchOptions { MaxResults = (uint)maxResults };
        
        var result = Runtime.Lobby.LobbyInterface.CreateLobbySearch(ref createLobbySearchOptions, out var searchHandle);
        if (result != Result.Success || searchHandle == null)
        {
            FusionLogger.Error($"Failed to create lobby search: {result}");
            callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
            yield break;
        }
        
        SetParameter(ref searchHandle, LobbyKeys.HasLobbyOpenKey, bool.TrueString, ComparisonOp.Equal);
        SetParameter(ref searchHandle, LobbyKeys.IdentifierKey, bool.TrueString, ComparisonOp.Equal);
        SetParameter(ref searchHandle, LobbyKeys.GameKey, GameInfo.GameName, ComparisonOp.Equal);
        
        if (string.IsNullOrWhiteSpace(code))
        {
            SetParameter(ref searchHandle, LobbyKeys.PrivacyKey, ((int)ServerPrivacy.PUBLIC).ToString(), ComparisonOp.Equal);
            
            if (filters.FilterFull)
            {
                SetParameter(ref searchHandle, LobbyKeys.FullKey, bool.FalseString, ComparisonOp.Equal);
            }
            
            if (filters.FilterMismatchingVersions)
            {
                var version = FusionMod.Version;
                SetParameter(ref searchHandle, LobbyKeys.VersionMajorKey, version.Major.ToString(), ComparisonOp.Equal);
                SetParameter(ref searchHandle, LobbyKeys.VersionMinorKey, version.Minor.ToString(), ComparisonOp.Equal);
            }
        }
        else
        {
            SetParameter(ref searchHandle, LobbyKeys.LobbyCodeKey, code.ToUpper(), ComparisonOp.Equal);
        }
        
        var lobbySearchFindOptions = new LobbySearchFindOptions { LocalUserId = LocalUserId };
        
        Result searchResult = Result.Success;
        bool searchComplete = false;
        searchHandle.Find(ref lobbySearchFindOptions, null, (ref LobbySearchFindCallbackInfo info) =>
        {
            searchResult = info.ResultCode;
            searchComplete = true;
        });
        
        var timeoutAt = DateTime.UtcNow.AddSeconds(SearchTimeoutSeconds);
        while (!searchComplete && DateTime.UtcNow < timeoutAt)
            yield return null;
        
        if (!searchComplete)
        {
            FusionLogger.Error("EOS lobby search timed out");
            searchHandle.Release();
            callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
            yield break;
        }

        if (searchResult != Result.Success)
        {
            FusionLogger.Error($"EOS lobby search failed: {searchResult}");
            searchHandle.Release();
            callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
            yield break;
        }
        
        var countOptions = new LobbySearchGetSearchResultCountOptions();
        uint lobbyCount = searchHandle.GetSearchResultCount(ref countOptions);
        
#if DEBUG
        FusionLogger.Log($"Lobbies Found: {lobbyCount}");
#endif
        
        List<IMatchmaker.LobbyInfo> netLobbies = new((int)lobbyCount);

        for (uint i = 0; i < lobbyCount; i++)
        {
            var copyOptions = new LobbySearchCopySearchResultByIndexOptions { LobbyIndex = i };

            if (searchHandle.CopySearchResultByIndex(ref copyOptions, out var lobbyDetails) != Result.Success || lobbyDetails == null)
                continue;
            
            var infoOptions = new LobbyDetailsCopyInfoOptions();
            if (lobbyDetails.CopyInfo(ref infoOptions, out var lobbyInfo) != Result.Success || !lobbyInfo.HasValue)
            {
                lobbyDetails.Release();
                continue;
            }

            if (lobbyInfo.Value.LobbyOwnerUserId == null)
            {
                lobbyDetails.Release();
                continue;
            }

            var networkLobby = new EpicLobby(Runtime, lobbyDetails, lobbyInfo.Value.LobbyId, lobbyInfo.Value.LobbyOwnerUserId);
            var metadata = LobbyMetadataSerializer.ReadInfo(networkLobby);

#if !DEBUG
            if (metadata.LobbyInfo?.LobbyHostID == LocalUserId.ToString())
            {
                lobbyDetails.Release();
                continue;
            }
#endif

            if (!metadata.HasLobbyOpen)
            {
                lobbyDetails.Release();
                continue;
            }

            netLobbies.Add(new IMatchmaker.LobbyInfo
            {
                Lobby = networkLobby,
                Metadata = metadata,
            });
        }

        searchHandle.Release();

        var callbackInfo = new IMatchmaker.MatchmakerCallbackInfo { Lobbies = netLobbies.ToArray() };
        callback?.Invoke(callbackInfo);
    }

    private void SetParameter(ref LobbySearch searchHandle, string key, string value, ComparisonOp comparisonOp)
    {
        var lobbySearchSetParameterOptions = new LobbySearchSetParameterOptions
        {
            Parameter = new AttributeData
            {
                Key = key,
                Value = value,
            },
            ComparisonOp = comparisonOp,
        };
        
        var result = searchHandle.SetParameter(ref lobbySearchSetParameterOptions);
        if (result != Result.Success)
        {
            FusionLogger.Error($"Failed to set lobby search parameter: {result}");
        }
    }
}