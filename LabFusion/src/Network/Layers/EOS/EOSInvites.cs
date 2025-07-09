using Epic.OnlineServices;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.Lobby;

using LabFusion.UI.Popups;
using LabFusion.Utilities;

using MelonLoader;

using System.Collections;

namespace LabFusion.Network;

internal class EOSInvites
{
	private static ulong _lobbyInviteReceivedNotificationId = Common.InvalidNotificationid;
	private static ulong _lobbyInviteAcceptedNotificationId = Common.InvalidNotificationid;

	private static ulong _friendStatusUpdateNotificationId = Common.InvalidNotificationid;
	private static ulong _friendInviteAcceptedNotificationId = Common.InvalidNotificationid;

	internal static void ConfigureInvites()
	{
		var addNotifyLobbyInviteReceivedOptions = new AddNotifyLobbyInviteReceivedOptions();
		_lobbyInviteReceivedNotificationId = EOSManager.LobbyInterface.AddNotifyLobbyInviteReceived(ref addNotifyLobbyInviteReceivedOptions, null, OnLobbyInviteReceived);

		var addNotifyLobbyInviteAcceptedOptions = new AddNotifyLobbyInviteAcceptedOptions();
		_lobbyInviteAcceptedNotificationId = EOSManager.LobbyInterface.AddNotifyLobbyInviteAccepted(ref addNotifyLobbyInviteAcceptedOptions, null, OnLobbyInviteAccepted);

		var addNotifyFriendsUpdateOptions = new AddNotifyFriendsUpdateOptions();
		_friendStatusUpdateNotificationId = EOSManager.FriendsInterface.AddNotifyFriendsUpdate(ref addNotifyFriendsUpdateOptions, null, OnFriendStatusUpdate);
	}

	internal static void ShutdownInvites()
	{
		if (_lobbyInviteReceivedNotificationId != Common.InvalidNotificationid)
		{
			EOSManager.LobbyInterface.RemoveNotifyLobbyInviteReceived(_lobbyInviteReceivedNotificationId);
			_lobbyInviteReceivedNotificationId = Common.InvalidNotificationid;
		}
		if (_lobbyInviteAcceptedNotificationId != Common.InvalidNotificationid)
		{
			EOSManager.LobbyInterface.RemoveNotifyLobbyInviteAccepted(_lobbyInviteAcceptedNotificationId);
			_lobbyInviteAcceptedNotificationId = Common.InvalidNotificationid;
		}

		if (_friendStatusUpdateNotificationId != Common.InvalidNotificationid)
		{
			EOSManager.FriendsInterface.RemoveNotifyFriendsUpdate(_friendStatusUpdateNotificationId);
			_friendStatusUpdateNotificationId = Common.InvalidNotificationid;
		}
		if (_friendInviteAcceptedNotificationId != Common.InvalidNotificationid)
		{
			EOSManager.FriendsInterface.RemoveNotifyFriendsUpdate(_friendInviteAcceptedNotificationId);
			_friendInviteAcceptedNotificationId = Common.InvalidNotificationid;
		}
	}

	private static void OnLobbyInviteReceived(ref LobbyInviteReceivedCallbackInfo inviteInfo)
	{
		FusionLogger.Log($"Received lobby invite: {inviteInfo.InviteId}");
		string senderName;
		string lobbyId;
		string inviteId = inviteInfo.InviteId;

		var copyLobbyDetailsOptions = new CopyLobbyDetailsHandleByInviteIdOptions()
		{
			InviteId = inviteInfo.InviteId,
		};
		Result result = EOSManager.LobbyInterface.CopyLobbyDetailsHandleByInviteId(ref copyLobbyDetailsOptions, out LobbyDetails lobbyDetailsHandle);

		if (result == Result.Success)
		{
			var lobbyDetailsCopyInfoOptions = new LobbyDetailsCopyInfoOptions();
			Result copyLobbyInfoResult = lobbyDetailsHandle.CopyInfo(ref lobbyDetailsCopyInfoOptions, out LobbyDetailsInfo? lobbyDetailsInfo);
			senderName = EOSUtils.GetDisplayNameFromProductId(inviteInfo.TargetUserId);
			lobbyId = lobbyDetailsInfo?.LobbyId;
		}
		else
		{
			FusionLogger.Error($"Failed to copy lobby details by invite ID: {result}");
			return;
		}

		Notifier.Send(new Notification()
		{
			Title = $"Lobby Invite Received!",
			Message = new NotificationText($"{senderName} has invited you to join their lobby!"),
			Type = NotificationType.INFORMATION,
			SaveToMenu = true,
			ShowPopup = true,
			OnAccepted = () =>
			{
				FusionLogger.Log($"Accepted lobby invite: {inviteId} for lobby {lobbyId}");
				if (NetworkLayerManager.Layer is EOSNetworkLayer eosLayer)
				{
					var networkLayer = NetworkLayerManager.Layer as EOSNetworkLayer;
					networkLayer.JoinServer(lobbyId);
				}
			},
		});
	}
	private static void OnLobbyInviteAccepted(ref LobbyInviteAcceptedCallbackInfo inviteInfo)
	{
		// Used for accepting invites via the overlay
		FusionLogger.Log($"Accepted lobby invite: {inviteInfo.InviteId} for lobby {inviteInfo.LobbyId}");
		if (NetworkLayerManager.Layer is EOSNetworkLayer eosLayer)
		{
			var networkLayer = NetworkLayerManager.Layer as EOSNetworkLayer;
			networkLayer.JoinServer(inviteInfo.LobbyId);
		}
	}

	private static void OnFriendStatusUpdate(ref OnFriendsUpdateInfo updateInfo)
	{
		switch (updateInfo.CurrentStatus)
		{
			case FriendsStatus.InviteReceived:
				MelonCoroutines.Start(InviteReceived(updateInfo));
				break;
			case FriendsStatus.Friends:
				MelonCoroutines.Start(InviteAccepted(updateInfo));
				break;
			default:
				return;
		}

		IEnumerator InviteReceived(OnFriendsUpdateInfo updateInfo)
		{
			string senderName = "Unknown";
			var acceptInviteOptions = new AcceptInviteOptions()
			{
				TargetUserId = updateInfo.TargetUserId,
				LocalUserId = updateInfo.LocalUserId
			};

			bool complete = false;
			MelonCoroutines.Start(EOSUtils.GetDisplayNameFromAccountId(updateInfo.TargetUserId, (username) =>
			{
				complete = true;
				senderName = username ?? "Unknown";
			}));

			while (!complete)
				yield return null;

			Notifier.Send(new Notification()
			{
				Title = $"Friend Request Received!",
				Message = new NotificationText($"{senderName} has sent you a friend request!"),
				Type = NotificationType.INFORMATION,
				SaveToMenu = true,
				ShowPopup = true,
				OnAccepted = () =>
				{
					FusionLogger.Log($"Accepted friend request: {senderName}");
					EOSManager.FriendsInterface.AcceptInvite(ref acceptInviteOptions, null, null);
				},
			});

			yield return null;
		}

		IEnumerator InviteAccepted(OnFriendsUpdateInfo updateInfo)
		{
			string senderName = "Unknown";

			bool complete = false;
			MelonCoroutines.Start(EOSUtils.GetDisplayNameFromAccountId(updateInfo.TargetUserId, (username) =>
			{
				complete = true;
				senderName = username ?? "Unknown";
			}));

			while (!complete)
				yield return null;

			Notifier.Send(new Notification()
			{
				Title = $"Friend Request Accepted!",
				Message = new NotificationText($"You are now friends with {senderName}!"),
				Type = NotificationType.SUCCESS,
				SaveToMenu = true,
				ShowPopup = true,
			});

			yield return null;
		}
	}
}