using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using LabFusion.Player;
using LabFusion.Utilities;

using MelonLoader;

using System.Collections;
using System.Collections.Concurrent;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// EOS lobby searching and matchmaking. 
/// </summary>
public class EOSMatchmaker : IMatchmaker
{
    private const int DefaultMaxResults = 200;
    private const int CodeSearchMaxResults = 10;
    private const float CacheValiditySeconds = 5f;

    // Cache
    private static readonly object _cacheLock = new();
    private static IMatchmaker.LobbyInfo[] _cachedLobbies;
    private static DateTime _cacheTime = DateTime.MinValue;
    private static bool _isSearching;

    private static readonly LobbySearchSetParameterOptions ServerOpenParam = new()
    {
        Parameter = new AttributeData
        {
            Key = LobbyKeys.HasLobbyOpenKey,
            Value = bool.TrueString,
        },
        ComparisonOp = ComparisonOp.Equal,
    };

    public void RequestLobbies(Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        RequestLobbies(MatchmakerFilters.Empty, callback);
    }

    public void RequestLobbies(MatchmakerFilters filters, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        // Check cache first
        if (TryGetCachedLobbies(out var cached))
        {
#if DEBUG
            FusionLogger.Log($"Returning {cached.Length} cached lobbies");
#endif
            callback?.Invoke(new IMatchmaker.MatchmakerCallbackInfo { Lobbies = cached });
            return;
        }

        MelonCoroutines.Start(SearchLobbiesOptimized(null, filters, callback));
    }

    public void RequestLobbiesByCode(string code, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        MelonCoroutines.Start(SearchByCodeOptimized(code, callback));
    }

    /// <summary>
    /// Clears the lobby cache, forcing a fresh search on next request.
    /// </summary>
    public static void InvalidateCache()
    {
        lock (_cacheLock)
        {
            _cachedLobbies = null;
            _cacheTime = DateTime.MinValue;
        }
    }

    private static bool TryGetCachedLobbies(out IMatchmaker.LobbyInfo[] lobbies)
    {
        lock (_cacheLock)
        {
            if (_cachedLobbies != null &&
                (DateTime.UtcNow - _cacheTime).TotalSeconds < CacheValiditySeconds)
            {
                lobbies = _cachedLobbies;
                return true;
            }
        }

        lobbies = null;
        return false;
    }

    private static void UpdateCache(IMatchmaker.LobbyInfo[] lobbies)
    {
        lock (_cacheLock)
        {
            _cachedLobbies = lobbies;
            _cacheTime = DateTime.UtcNow;
        }
    }

    private IEnumerator SearchByCodeOptimized(string code, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
            yield break;
        }

#if DEBUG
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
#endif

        var searchHandle = CreateSearchHandle(CodeSearchMaxResults);
        if (searchHandle == null)
        {
            callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
            yield break;
        }

        // Set code-specific search parameter
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

        // Execute search
        bool searchComplete = false;
        Result searchResult = Result.Success;

        ExecuteSearch(searchHandle, (result) =>
        {
            searchResult = result;
            searchComplete = true;
        });

        while (!searchComplete)
            yield return null;

        if (searchResult != Result.Success)
        {
            FusionLogger.Error($"Code search failed: {searchResult}");
            searchHandle.Release();
            callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
            yield break;
        }

        // Process results - for code search we only need the first valid match
        var lobby = FindFirstValidLobby(searchHandle);
        searchHandle.Release();

#if DEBUG
        stopwatch.Stop();
        FusionLogger.Log($"Code search completed in {stopwatch.ElapsedMilliseconds}ms, found:  {(lobby.HasValue ? "Yes" : "No")}");
#endif

        var result = lobby.HasValue
            ? new IMatchmaker.MatchmakerCallbackInfo { Lobbies = new[] { lobby.Value } }
            : IMatchmaker.MatchmakerCallbackInfo.Empty;

        callback?.Invoke(result);
    }

    private IEnumerator SearchLobbiesOptimized(string code, MatchmakerFilters filters, Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        // Prevent concurrent searches
        lock (_cacheLock)
        {
            if (_isSearching)
            {
                // Wait for existing search to complete and use its results
                yield return WaitForExistingSearch(callback);
                yield break;
            }
            _isSearching = true;
        }

#if DEBUG
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
#endif

        try
        {
            // Create search handle
            var searchHandle = CreateSearchHandle(DefaultMaxResults);
            if (searchHandle == null)
            {
                callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
                yield break;
            }

            // Set search parameters
            ConfigureSearchParameters(searchHandle, code, filters);

            // Execute search
            bool searchComplete = false;
            Result searchResult = Result.Success;

            ExecuteSearch(searchHandle, (result) =>
            {
                searchResult = result;
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

            // Process results in parallel batches
            var lobbies = ProcessSearchResults(searchHandle);
            searchHandle.Release();

            // Update cache
            var lobbiesArray = lobbies.ToArray();
            UpdateCache(lobbiesArray);

#if DEBUG
            stopwatch.Stop();
            FusionLogger.Log($"Lobby search completed in {stopwatch.ElapsedMilliseconds}ms, found {lobbiesArray.Length} lobbies");
#endif

            callback?.Invoke(new IMatchmaker.MatchmakerCallbackInfo { Lobbies = lobbiesArray });
        }
        finally
        {
            lock (_cacheLock)
            {
                _isSearching = false;
            }
        }
    }

    private IEnumerator WaitForExistingSearch(Action<IMatchmaker.MatchmakerCallbackInfo> callback)
    {
        // Wait for the existing search to complete
        while (true)
        {
            lock (_cacheLock)
            {
                if (!_isSearching)
                    break;
            }
            yield return null;
        }

        // Use cached results
        if (TryGetCachedLobbies(out var cached))
        {
            callback?.Invoke(new IMatchmaker.MatchmakerCallbackInfo { Lobbies = cached });
        }
        else
        {
            callback?.Invoke(IMatchmaker.MatchmakerCallbackInfo.Empty);
        }
    }

    private static LobbySearch CreateSearchHandle(int maxResults)
    {
        if (EOSInterfaces.Lobby == null)
        {
            FusionLogger.Error("LobbyInterface is null");
            return null;
        }

        var createOptions = new CreateLobbySearchOptions
        {
            MaxResults = (uint)maxResults,
        };

        var result = EOSInterfaces.Lobby.CreateLobbySearch(ref createOptions, out var searchHandle);

        if (result != Result.Success || searchHandle == null)
        {
            FusionLogger.Error($"Failed to create lobby search:  {result}");
            return null;
        }

        return searchHandle;
    }

    private static void ConfigureSearchParameters(LobbySearch searchHandle, string code, MatchmakerFilters filters)
    {
        // Always filter for open servers
        var openParam = ServerOpenParam;
        searchHandle.SetParameter(ref openParam);

        // Add code filter if specified
        if (!string.IsNullOrEmpty(code))
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

        // Add additional filters as needed
        // Note: EOS search parameters are limited, complex filtering happens post-search
    }

    private static void ExecuteSearch(LobbySearch searchHandle, Action<Result> onComplete)
    {
        var localUserId = ProductUserId.FromString(PlayerIDManager.LocalPlatformID);
        if (localUserId == null)
        {
            onComplete?.Invoke(Result.InvalidUser);
            return;
        }

        var findOptions = new LobbySearchFindOptions
        {
            LocalUserId = localUserId
        };

        searchHandle.Find(ref findOptions, null, (ref LobbySearchFindCallbackInfo info) =>
        {
            onComplete?.Invoke(info.ResultCode);
        });
    }

    private static List<IMatchmaker.LobbyInfo> ProcessSearchResults(LobbySearch searchHandle)
    {
        var countOptions = new LobbySearchGetSearchResultCountOptions();
        uint lobbyCount = searchHandle.GetSearchResultCount(ref countOptions);

        if (lobbyCount == 0)
            return new List<IMatchmaker.LobbyInfo>();

        // Pre-allocate with expected capacity
        var validLobbies = new ConcurrentBag<IMatchmaker.LobbyInfo>();
        var lobbyDetailsToProcess = new List<(uint Index, LobbyDetails Details)>((int)lobbyCount);

        // First pass: Copy all lobby details (fast)
        for (uint i = 0; i < lobbyCount; i++)
        {
            var copyOptions = new LobbySearchCopySearchResultByIndexOptions
            {
                LobbyIndex = i
            };

            if (searchHandle.CopySearchResultByIndex(ref copyOptions, out var lobbyDetails) == Result.Success &&
                lobbyDetails != null)
            {
                lobbyDetailsToProcess.Add((i, lobbyDetails));
            }
        }

        // Second pass: Process lobby details (can be parallelized, but EOS callbacks are single-threaded)
        foreach (var (index, lobbyDetails) in lobbyDetailsToProcess)
        {
            var lobbyInfo = ProcessSingleLobby(lobbyDetails);
            if (lobbyInfo.HasValue)
            {
                validLobbies.Add(lobbyInfo.Value);
            }
            else
            {
                lobbyDetails.Release();
            }
        }

        return validLobbies.ToList();
    }

    private static IMatchmaker.LobbyInfo? ProcessSingleLobby(LobbyDetails lobbyDetails)
    {
        // Quick validation: Check owner exists
        var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();
        var ownerId = lobbyDetails.GetLobbyOwner(ref ownerOptions);

        if (ownerId == null)
            return null; // Dead lobby

        // Get lobby info
        var infoOptions = new LobbyDetailsCopyInfoOptions();
        if (lobbyDetails.CopyInfo(ref infoOptions, out var lobbyInfo) != Result.Success || !lobbyInfo.HasValue)
            return null;

        var networkLobby = new EpicLobby(lobbyDetails, lobbyInfo.Value.LobbyId);

        // Validate server is open
        if (!networkLobby.TryGetMetadata(LobbyKeys.HasLobbyOpenKey, out var hasServerOpen) ||
            hasServerOpen != bool.TrueString)
            return null;

        // Read metadata
        var metadata = LobbyMetadataSerializer.ReadInfo(networkLobby);

#if !DEBUG
        if (metadata.LobbyInfo.LobbyHostID == PlayerIDManager.LocalPlatformID)
            return null;
#endif

        if (!metadata.HasLobbyOpen)
            return null;

        return new IMatchmaker.LobbyInfo
        {
            Lobby = networkLobby,
            Metadata = metadata
        };
    }

    private static IMatchmaker.LobbyInfo? FindFirstValidLobby(LobbySearch searchHandle)
    {
        var countOptions = new LobbySearchGetSearchResultCountOptions();
        uint lobbyCount = searchHandle.GetSearchResultCount(ref countOptions);

        for (uint i = 0; i < lobbyCount; i++)
        {
            var copyOptions = new LobbySearchCopySearchResultByIndexOptions
            {
                LobbyIndex = i
            };

            if (searchHandle.CopySearchResultByIndex(ref copyOptions, out var lobbyDetails) != Result.Success ||
                lobbyDetails == null)
                continue;

            var result = ProcessSingleLobby(lobbyDetails);

            if (result.HasValue)
                return result;

            lobbyDetails.Release();
        }

        return null;
    }
}