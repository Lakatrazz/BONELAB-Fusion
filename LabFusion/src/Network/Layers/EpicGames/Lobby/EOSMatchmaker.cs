using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using LabFusion.Player;
using LabFusion.Support;
using LabFusion.Utilities;
using MelonLoader;
using System.Collections;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// EOS lobby searching and matchmaking. 
/// </summary>
internal class EOSMatchmaker : IMatchmaker
{
    private const int DefaultMaxResults = 200;
    private const int CodeSearchMaxResults = 1;

    public void RequestLobbies(Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        RequestLobbies(MatchmakerFilters.Empty, callback);
    }

    public void RequestLobbies(MatchmakerFilters filters, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        MelonCoroutines.Start(FindLobbies(DefaultMaxResults, null, filters, callback));
    }

    public void RequestLobbiesByCode(string code, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        MelonCoroutines.Start(FindLobbies(CodeSearchMaxResults, code, MatchmakerFilters.Empty, callback));
    }

    private static IEnumerator FindLobbies(int maxResults, string code, MatchmakerFilters filters, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        if (EOSInterfaces.Lobby == null)
        {
            FusionLogger.Error("LobbyInterface is null");
            callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
            yield break;
        }

        // Create search handle
        var createOptions = new CreateLobbySearchOptions { MaxResults = (uint)maxResults };
        var result = EOSInterfaces.Lobby.CreateLobbySearch(ref createOptions, out var searchHandle);

        if (result != Result.Success || searchHandle == null)
        {
            FusionLogger.Error($"Failed to create lobby search: {result}");
            callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
            yield break;
        }

        var openParam = new LobbySearchSetParameterOptions()
        {
            Parameter = new AttributeData
            {
                Key = LobbyKeys.HasLobbyOpenKey,
                Value = bool.TrueString,
            },
            ComparisonOp = ComparisonOp.Equal,
        };
        searchHandle.SetParameter(ref openParam);

        var identifierParam = new LobbySearchSetParameterOptions
        {
            Parameter = new AttributeData
            {
                Key = LobbyKeys.IdentifierKey,
                Value = bool.TrueString,
            },
            ComparisonOp = ComparisonOp.Equal,
        };
        searchHandle.SetParameter(ref identifierParam);

        var gameParam = new LobbySearchSetParameterOptions
        {
            Parameter = new AttributeData
            {
                Key = LobbyKeys.GameKey,
                Value = GameInfo.GameName,
            },
            ComparisonOp = ComparisonOp.Equal,
        };
        searchHandle.SetParameter(ref gameParam);

        // Filter out private and locked lobbies unless searching by code
        if (string.IsNullOrWhiteSpace(code))
        {
            var notPrivateParam = new LobbySearchSetParameterOptions
            {
                Parameter = new AttributeData
                {
                    Key = LobbyKeys.PrivacyKey,
                    Value = ((int)ServerPrivacy.PRIVATE).ToString(),
                },
                ComparisonOp = ComparisonOp.Notequal,
            };
            searchHandle.SetParameter(ref notPrivateParam);

            var notLockedParam = new LobbySearchSetParameterOptions
            {
                Parameter = new AttributeData
                {
                    Key = LobbyKeys.PrivacyKey,
                    Value = ((int)ServerPrivacy.LOCKED).ToString(),
                },
                ComparisonOp = ComparisonOp.Notequal,
            };
            searchHandle.SetParameter(ref notLockedParam);

            if (filters.FilterFull)
            {
                var notFullParam = new LobbySearchSetParameterOptions
                {
                    Parameter = new AttributeData
                    {
                        Key = LobbyKeys.FullKey,
                        Value = bool.FalseString,
                    },
                    ComparisonOp = ComparisonOp.Equal,
                };
                searchHandle.SetParameter(ref notFullParam);
            }

            if (filters.FilterMismatchingVersions)
            {
                var version = FusionMod.Version;

                var versionMajorParam = new LobbySearchSetParameterOptions
                {
                    Parameter = new AttributeData
                    {
                        Key = LobbyKeys.VersionMajorKey,
                        Value = version.Major.ToString(),
                    },
                    ComparisonOp = ComparisonOp.Equal,
                };
                searchHandle.SetParameter(ref versionMajorParam);

                var versionMinorParam = new LobbySearchSetParameterOptions
                {
                    Parameter = new AttributeData
                    {
                        Key = LobbyKeys.VersionMinorKey,
                        Value = version.Minor.ToString(),
                    },
                    ComparisonOp = ComparisonOp.Equal,
                };
                searchHandle.SetParameter(ref versionMinorParam);
            }
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            var codeParam = new LobbySearchSetParameterOptions
            {
                Parameter = new AttributeData
                {
                    Key = LobbyKeys.LobbyCodeKey,
                    Value = code.ToUpperInvariant(),
                },
                ComparisonOp = ComparisonOp.Equal,
            };
            searchHandle.SetParameter(ref codeParam);
        }

        // Execute search
        var localUserId = ProductUserId.FromString(PlayerIDManager.LocalPlatformID);
        if (localUserId == null)
        {
            FusionLogger.Error("Local user ID is null");
            searchHandle.Release();
            callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
            yield break;
        }

        bool searchComplete = false;
        Result searchResult = Result.Success;

        var findOptions = new LobbySearchFindOptions { LocalUserId = localUserId };
        searchHandle.Find(ref findOptions, null, (ref LobbySearchFindCallbackInfo info) =>
        {
            searchResult = info.ResultCode;
            searchComplete = true;
        });

        while (!searchComplete)
            yield return null;

        if (searchResult != Result.Success)
        {
            FusionLogger.Error($"Lobby search failed: {searchResult}");
            searchHandle.Release();
            callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
            yield break;
        }

        // Process results
        var countOptions = new LobbySearchGetSearchResultCountOptions();
        uint lobbyCount = searchHandle.GetSearchResultCount(ref countOptions);
        
#if DEBUG
        FusionLogger.Log($"Lobbies Found: {lobbyCount}");
#endif

        List<IMatchmaker.LobbyInfo> netLobbies = new();

        for (uint i = 0; i < lobbyCount; i++)
        {
            var copyOptions = new LobbySearchCopySearchResultByIndexOptions { LobbyIndex = i };

            if (searchHandle.CopySearchResultByIndex(ref copyOptions, out var lobbyDetails) != Result.Success || lobbyDetails == null)
                continue;

            // Check owner exists
            var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            var ownerId = lobbyDetails.GetLobbyOwner(ref ownerOptions);

            if (ownerId == null)
            {
                lobbyDetails.Release();
                continue;
            }

            // Get lobby ID
            var infoOptions = new LobbyDetailsCopyInfoOptions();
            if (lobbyDetails.CopyInfo(ref infoOptions, out var lobbyInfo) != Result.Success || !lobbyInfo.HasValue)
            {
                lobbyDetails.Release();
                continue;
            }

            var networkLobby = new EpicLobby(lobbyDetails, lobbyInfo.Value.LobbyId);
            var metadata = LobbyMetadataSerializer.ReadInfo(networkLobby);

#if !DEBUG
            if (metadata.LobbyInfo.LobbyHostID == PlayerIDManager.LocalPlatformID)
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

        callback?.Invoke(new IMatchmaker.MatchmakerCallbackInfo { Lobbies = netLobbies.ToArray() });
    }
}