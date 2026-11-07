using Epic.OnlineServices;
using Epic.OnlineServices.Friends;

namespace MarrowFusion.Epic;

internal class EOSFriends : EOSInterface
{
    internal EOSRuntime Runtime;
    internal FriendsInterface FriendsInterface;
    internal EpicAccountId LocalAccountId;
    
    private List<EpicAccountId> friendsList = new();
    
    internal EOSFriends(EOSRuntime eosRuntime, FriendsInterface friendsInterface, EpicAccountId localAccountId)
    {
        Runtime = eosRuntime;
        FriendsInterface = friendsInterface;
        LocalAccountId = localAccountId;
    }

    internal override async Task<bool> InitializeAsync()
    {
        friendsList = await GetFriendsListAsync();
        
        return true;
    }

    internal async Task<List<EpicAccountId>> GetFriendsListAsync()
    {
        friendsList = new List<EpicAccountId>();
        
        var tcs = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        
        var queryOptions = new QueryFriendsOptions
        {
            LocalUserId = LocalAccountId
        };
        
        FriendsInterface.QueryFriends(ref queryOptions, null, (ref QueryFriendsCallbackInfo info) =>
        {
            tcs.TrySetResult(info.ResultCode);
        });
        
        Result result = await tcs.Task;
        
        if (result != Result.Success)
        {
            EpicModule.Logger.Error($"Failed to query friends list: {result}");
            return friendsList;
        }
        
        var countOptions = new GetFriendsCountOptions
        {
            LocalUserId = LocalAccountId
        };
        
        int count = FriendsInterface.GetFriendsCount(ref countOptions);
        
        for (int i = 0; i < count; i++)
        {
            var indexOptions = new GetFriendAtIndexOptions
            {
                LocalUserId = LocalAccountId,
                Index = i
            };

            EpicAccountId friendId = FriendsInterface.GetFriendAtIndex(ref indexOptions);

            if (friendId == null || !friendId.IsValid())
                continue;
            
            var statusOptions = new GetStatusOptions
            {
                LocalUserId = LocalAccountId,
                TargetUserId = friendId
            };

            if (FriendsInterface.GetStatus(ref statusOptions) == FriendsStatus.Friends)
                friendsList.Add(friendId);
        }
        
        return friendsList;
    }
    
    internal async Task<bool> IsFriendAsync(EpicAccountId accountId)
    {
        return friendsList.Contains(accountId);
    }
}