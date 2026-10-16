using Epic.OnlineServices;
using Epic.OnlineServices.UserInfo;

namespace MarrowFusion.Epic;

internal class EOSUserInfo : EOSInterface
{
    internal EOSRuntime Runtime;
    internal UserInfoInterface UserInfoInterface;
    internal EpicAccountId LocalAccountId;
    
    internal EOSUserInfo(EOSRuntime eosRuntime, UserInfoInterface userInfoInterface, EpicAccountId localAccountId)
    {
        Runtime = eosRuntime;
        UserInfoInterface = userInfoInterface;
        LocalAccountId = localAccountId;
    }

    internal async Task<string> GetDisplayNameAsync(EpicAccountId accountId)
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        var options = new QueryUserInfoOptions
        {
            LocalUserId = LocalAccountId,
            TargetUserId = accountId
        };

        UserInfoInterface.QueryUserInfo(ref options, null, (ref QueryUserInfoCallbackInfo data) =>
        {
            if (data.ResultCode != Result.Success)
            {
                tcs.TrySetResult(null);
                return;
            }

            var copyOptions = new CopyUserInfoOptions
            {
                LocalUserId = LocalAccountId,
                TargetUserId = accountId
            };

            var result = UserInfoInterface.CopyUserInfo(ref copyOptions, out var userData);

            tcs.TrySetResult(result == Result.Success && userData.HasValue ? userData.Value.DisplayName : "Player");
        });

        return await tcs.Task;
    }
    
    internal async Task<string> GetDisplayNameAsync(ProductUserId productUserId)
    {
        EpicAccountId epicAccountId = await Runtime.Connect.GetEpicAccountIdAsync(productUserId);

        if (epicAccountId == null)
            return null;

        return await Runtime.UserInfo.GetDisplayNameAsync(epicAccountId);
    }
}