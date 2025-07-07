using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

using LabFusion.Utilities;

namespace LabFusion.Network;

public class EOSUtils
{
	public static EpicAccountId GetAccountIdFromProductId(ProductUserId productUserId)
	{
		if (productUserId == null)
			return null;

		var options = new CopyProductUserExternalAccountByAccountTypeOptions
		{
			TargetUserId = productUserId,
			AccountIdType = ExternalAccountType.Epic,
		};

		Result result = EOSManager.ConnectInterface.CopyProductUserExternalAccountByAccountType(ref options, out Epic.OnlineServices.Connect.ExternalAccountInfo? externalAccountInfo);

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

	public static string GetDisplayNameFromProductId(ProductUserId productUserId)
	{
		if (productUserId == null)
			return null;
		var options = new CopyProductUserExternalAccountByAccountTypeOptions
		{
			TargetUserId = productUserId,
			AccountIdType = ExternalAccountType.Epic,
		};
		Result result = EOSManager.ConnectInterface.CopyProductUserExternalAccountByAccountType(ref options, out Epic.OnlineServices.Connect.ExternalAccountInfo? externalAccountInfo);
		if (result == Result.Success && externalAccountInfo.HasValue)
		{
			return externalAccountInfo.Value.DisplayName ?? "Unknown";
		}
		else if (result != Result.Success)
		{
			FusionLogger.Warn($"Failed to get display name for ProductUserId {productUserId}: {result}");
			return null;
		}
		return null;
	}
}
