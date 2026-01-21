﻿using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

using LabFusion.Debugging;
using LabFusion.Utilities;

using MelonLoader;

using System.Collections;
using LabFusion.Network.EpicGames;
using LabFusion.Player;

namespace LabFusion.Network;

public class EOSMatchmaker : IMatchmaker
{
    private static readonly LobbySearchSetParameterOptions _serverOpenParamOptions = new()
    {
        Parameter = new AttributeData
        {
            Key = LobbyKeys.HasLobbyOpenKey,
            Value = bool.TrueString,
        },
        ComparisonOp = ComparisonOp.Equal,
    };

    public void RequestLobbies(MatchmakerFilters filters, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        MelonCoroutines.Start(FindLobbies(callback));
    }

    public void RequestLobbies(Action<IMatchmaker.MatchmakerCallbackInfo> callback) => MelonCoroutines.Start(FindLobbies(callback));
    public void RequestLobbiesByCode(string code, Action<IMatchmaker.MatchmakerCallbackInfo> callback) => MelonCoroutines.Start(FindLobbies(callback, code));

    private IEnumerator FindLobbies(Action<IMatchmaker.MatchmakerCallbackInfo> callback, string code = null)
    {
#if DEBUG
        FusionStopwatch.Create();
#endif

        bool noCodeProvided = string.IsNullOrEmpty(code);
        
        var createSearchOptions = new CreateLobbySearchOptions
        {
            MaxResults = 200,
        };

        Result createResult = EOSManager.LobbyInterface.CreateLobbySearch(ref createSearchOptions, out LobbySearch searchHandle);
        if (createResult != Result.Success || searchHandle == null)
        {
            FusionLogger.Error($"Failed to create lobby search: {createResult}");
            callback?.Invoke(new IMatchmaker.MatchmakerCallbackInfo { Lobbies = new IMatchmaker.LobbyInfo[0] });
            yield break;
        }

        LobbySearchSetParameterOptions paramOptions;
        if (noCodeProvided)
        {
            paramOptions = _serverOpenParamOptions;
        }
        else
        {
            paramOptions = new LobbySearchSetParameterOptions
            {
                Parameter = new AttributeData
                {
                    Key = LobbyKeys.LobbyCodeKey,
                    Value = code,
                },
                ComparisonOp = ComparisonOp.Equal,
            };
        }

        searchHandle.SetParameter(ref paramOptions);

        var findOptions = new LobbySearchFindOptions
        {
            LocalUserId = ProductUserId.FromString(PlayerIDManager.LocalPlatformID)
        };

        bool searchComplete = false;
        Result searchResult = Result.Success;

        searchHandle.Find(ref findOptions, null, (ref LobbySearchFindCallbackInfo info) =>
        {
            searchResult = info.ResultCode;
            searchComplete = true;
        });

        while (!searchComplete)
            yield return null;

        if (searchResult != Result.Success)
        {
            FusionLogger.Error($"Failed to find lobbies: {searchResult}");
            searchHandle.Release();
            callback?.Invoke(new IMatchmaker.MatchmakerCallbackInfo { Lobbies = new IMatchmaker.LobbyInfo[0] });
            yield break;
        }

        var lobbies = new List<IMatchmaker.LobbyInfo>();

        var countOptions = new LobbySearchGetSearchResultCountOptions();
        uint lobbyCount = searchHandle.GetSearchResultCount(ref countOptions);

        for (uint i = 0; i < lobbyCount; i++)
        {
            var copyOptions = new LobbySearchCopySearchResultByIndexOptions
            {
                LobbyIndex = i
            };

            if (searchHandle.CopySearchResultByIndex(ref copyOptions, out LobbyDetails lobbyDetails) != Result.Success || lobbyDetails == null)
            {
#if DEBUG
                FusionLogger.Error($"Failed to copy search result for lobby index {i}");
#endif
                continue;
            }

            var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            ProductUserId ownerId = lobbyDetails.GetLobbyOwner(ref ownerOptions);
            if (ownerId == null)
            {
                // More often than not, this means the lobby has been abandoned but the EOS backend is keeping it alive with 0 players.
#if DEBUG
                FusionLogger.Error($"Failed to get lobby owner for lobby index {i} since owner ID is null! Dead lobby?");
#endif
                lobbyDetails.Release();
                continue;
            }

            var infoOptions = new LobbyDetailsCopyInfoOptions();
            Result infoResult = lobbyDetails.CopyInfo(ref infoOptions, out LobbyDetailsInfo? lobbyInfo);

            if (infoResult != Result.Success || !lobbyInfo.HasValue)
            {
                lobbyDetails.Release();
                continue;
            }

            var networkLobby = new EpicLobby(lobbyDetails, lobbyInfo.Value.LobbyId);

            if (!networkLobby.TryGetMetadata(LobbyKeys.HasLobbyOpenKey, out var hasServerOpen) || hasServerOpen != bool.TrueString)
            {
                lobbyDetails.Release();
                continue;
            }

            var metadata = LobbyMetadataSerializer.ReadInfo(networkLobby);

#if !DEBUG
            if (metadata.LobbyInfo.LobbyId == EOSNetworkLayer.LocalUserId.ToString())
            {
                lobbyDetails.Release();
                continue;
            }
#endif

            if (metadata.HasLobbyOpen)
            {
                lobbies.Add(new IMatchmaker.LobbyInfo
                {
                    Lobby = networkLobby,
                    Metadata = metadata
                });
            }
            else
            {
                lobbyDetails.Release();
            }
        }

        searchHandle.Release();

#if DEBUG
        FusionStopwatch.Finish("matchmaking", out var ms);
#endif

        var resultInfo = new IMatchmaker.MatchmakerCallbackInfo
        {
            Lobbies = lobbies.ToArray()
        };

        callback?.Invoke(resultInfo);

#if DEBUG
        FusionLogger.Log($"Found {lobbies.Count} lobbies");
#endif
    }
}