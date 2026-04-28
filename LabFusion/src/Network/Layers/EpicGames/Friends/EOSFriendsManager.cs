using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using LabFusion.Utilities;
using System.Collections;

namespace LabFusion.Network.EpicGames;

internal class EOSFriendsManager
{
    private readonly ProductUserId _localUserId;
    private readonly EOSFriendsInterface friendsInterface;
    
    private readonly HashSet<ProductUserId> _friends = new();

    internal EOSFriendsManager(ProductUserId localUserId, ExternalAccountType accountType)
    {
        _localUserId = localUserId ?? throw new ArgumentNullException(nameof(localUserId));

        switch (accountType)
        {
            case ExternalAccountType.Steam:
                friendsInterface = new EOSSteamFriends();
                break;
            default:
                friendsInterface = new EOSDummyFriends();
                break;
        }
    }

    /// <summary>
    /// Fetches friends from the platform, resolves them to EOS ProductUserIds, and caches results.
    /// </summary>
    internal IEnumerator InitializeAsync()
    {
        IReadOnlyList<string> externalIds = null;
        yield return friendsInterface.GetFriendExternalIdsAsync(ids => externalIds = ids);

        if (externalIds == null || externalIds.Count == 0)
        {
#if DEBUG
            FusionLogger.Log($"No external friend IDs returned from {friendsInterface.AccountType}.");
#endif
            yield break;
        }

        yield return ResolveExternalIdsAsync(externalIds);
    }

    internal bool IsFriend(ProductUserId userId)
    {
        return  userId != null && userId == _localUserId || _friends.Contains(userId);
    }

    internal void Shutdown()
    {
        friendsInterface?.OnShutdown();
        _friends.Clear();
    }

    private IEnumerator ResolveExternalIdsAsync(IReadOnlyList<string> externalIds)
    {
        var connect = EOSInterfaces.Connect;
        if (connect == null)
        {
            FusionLogger.Error("ConnectInterface is null.");
            yield break;
        }

        bool finished = false;

        // EOS accepts up to 128 IDs per call
        const int chunkSize = 128;
        for (int i = 0; i < externalIds.Count; i += chunkSize)
        {
            var chunk = externalIds
                .Skip(i)
                .Take(chunkSize)
                .Select(id => (Utf8String)id)
                .ToArray();

            finished = false;

            var queryOptions = new QueryExternalAccountMappingsOptions
            {
                LocalUserId = _localUserId,
                AccountIdType = friendsInterface.AccountType,
                ExternalAccountIds = chunk,
            };

            connect.QueryExternalAccountMappings(ref queryOptions, null, (ref QueryExternalAccountMappingsCallbackInfo data) =>
            {
                if (data.ResultCode == Result.Success)
                {
                    foreach (Utf8String externalId in chunk)
                    {
                        var getOptions = new GetExternalAccountMappingsOptions
                        {
                            LocalUserId = _localUserId,
                            AccountIdType = friendsInterface.AccountType,
                            TargetExternalUserId = externalId,
                        };

                        var puid = connect.GetExternalAccountMapping(ref getOptions);
                        if (puid != null && puid.IsValid())
                        {
                            _friends.Add(puid);
                        }
                    }
                }
                else
                {
                    FusionLogger.Error($"QueryExternalAccountMappings failed: {data.ResultCode}");
                }

                finished = true;
            });

            while (!finished)
                yield return null;
        }
    }
}