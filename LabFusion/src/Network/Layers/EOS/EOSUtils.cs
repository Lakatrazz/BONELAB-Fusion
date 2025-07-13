using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.UserInfo;

using LabFusion.Utilities;

namespace LabFusion.Network;

internal class EOSUtils
{
    internal static EpicAccountId GetAccountIdFromProductId(ProductUserId productUserId)
    {
        if (productUserId == null)
            return null;

        var productUserIds = new ProductUserId[]
        {
            productUserId,
        };
        var queryOptions = new QueryProductUserIdMappingsOptions
        {
            LocalUserId = EOSNetworkLayer.LocalUserId,
            AccountIdType_DEPRECATED = ExternalAccountType.Epic,
            ProductUserIds = productUserIds
        };
        EOSManager.ConnectInterface.QueryProductUserIdMappings(ref queryOptions, null, null);

        var mappingsOptions = new GetProductUserIdMappingOptions
        {
            LocalUserId = EOSNetworkLayer.LocalUserId,
            AccountIdType = ExternalAccountType.Epic,
            TargetProductUserId = productUserId
        };
        EOSManager.ConnectInterface.GetProductUserIdMapping(ref mappingsOptions, out var epicAccountId);

        return EpicAccountId.FromString(epicAccountId);
    }

    internal static ProductUserId GetProductIdFromAccountId(EpicAccountId accountId)
    {
        if (accountId == null)
            return null;

        var epicAccountIds = new Utf8String[]
        {
            accountId.ToString()
        };
        var queryOptions = new QueryExternalAccountMappingsOptions
        {
            LocalUserId = EOSNetworkLayer.LocalUserId,
            AccountIdType = ExternalAccountType.Epic,
            ExternalAccountIds = epicAccountIds
        };
        EOSManager.ConnectInterface.QueryExternalAccountMappings(ref queryOptions, null, null);

        var mappingsOptions = new GetExternalAccountMappingsOptions
        {
            LocalUserId = EOSNetworkLayer.LocalUserId,
            AccountIdType = ExternalAccountType.Epic,
            TargetExternalUserId = accountId.ToString()
        };
        ProductUserId productUserId = EOSManager.ConnectInterface.GetExternalAccountMapping(ref mappingsOptions);

        return productUserId;
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
            return null;
        }

        return null;
    }

    internal static string GetDisplayNameFromAccountId(EpicAccountId accountId)
    {
        return GetDisplayNameFromProductId(GetProductIdFromAccountId(accountId));
    }
}
