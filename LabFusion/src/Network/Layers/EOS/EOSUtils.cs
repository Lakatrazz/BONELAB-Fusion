using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.UserInfo;

using LabFusion.Utilities;

using System.Collections;

namespace LabFusion.Network;

internal class EOSUtils
{
    internal static EpicAccountId GetAccountIdFromProductId(ProductUserId productUserId)
    {
        if (productUserId == null)
            return null;

        var options = new CopyProductUserExternalAccountByAccountTypeOptions
        {
            TargetUserId = productUserId,
            AccountIdType = ExternalAccountType.Epic,
        };

        Result result = EOSManager.ConnectInterface.CopyProductUserExternalAccountByAccountType(ref options, out ExternalAccountInfo? externalAccountInfo);

        if (result == Result.Success && externalAccountInfo.HasValue)
        {
            return EpicAccountId.FromString(externalAccountInfo.Value.AccountId);
        }
        else if (result != Result.Success)
        {
            FusionLogger.Warn($"Failed to get EpicAccountId for ProductUserId {productUserId}: {result}");
            return null;
        }

        return null;
    }

    internal static string GetDisplayNameFromProductId(ProductUserId productUserId)
    {
        if (productUserId == null)
            return null;

        var options = new CopyUserInfoOptions
        {
            TargetUserId = GetAccountIdFromProductId(productUserId),
            LocalUserId = EOSNetworkLayer.LocalAccountId
        };
        Result result = EOSManager.UserInfoInterface.CopyUserInfo(ref options, out UserInfoData? externalAccountInfo);
        if (result == Result.Success && externalAccountInfo.HasValue)
        {
            return externalAccountInfo?.DisplayName ?? "Unknown";
        }
        else if (result != Result.Success)
        {
            FusionLogger.Warn($"Failed to get display name for ProductUserId {productUserId}: {result}");
            return null;
        }

        return null;
    }

    internal static IEnumerator GetDisplayNameFromAccountId(EpicAccountId accountId, Action<string> onComplete)
    {
        var userInfoOptions = new QueryUserInfoOptions
        {
            LocalUserId = EOSNetworkLayer.LocalAccountId,
            TargetUserId = accountId
        };

        TaskCompletionSource<string> usernameTask = new TaskCompletionSource<string>();
        EOSManager.UserInfoInterface.QueryUserInfo(ref userInfoOptions, null, (ref QueryUserInfoCallbackInfo callbackInfo) =>
        {
            if (callbackInfo.ResultCode != Result.Success)
            {
                usernameTask.SetResult(string.Empty);
                return;
            }

            var copyOptions = new CopyUserInfoOptions
            {
                LocalUserId = EOSNetworkLayer.LocalAccountId,
                TargetUserId = accountId
            };

            if (EOSManager.UserInfoInterface.CopyUserInfo(ref copyOptions, out var userInfo) == Result.Success)
                usernameTask.SetResult(userInfo.Value.DisplayName ?? "Unknown");
        });

        while (!usernameTask.Task.IsCompleted)
            yield return null;

        onComplete?.Invoke(usernameTask.Task.Result);
        yield break;
    }
}
