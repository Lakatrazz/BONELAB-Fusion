using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.UI;

using LabFusion.UI.Popups;
using LabFusion.Utilities;

namespace LabFusion.Network;

internal class EOSOverlay
{
    private static ulong lobbyInviteReceivedNotificationId = Common.InvalidNotificationid;
    private static ulong lobbyInviteAcceptedNotificationId = Common.InvalidNotificationid;

    private static ulong joinLobbyAcceptedNotificationId = Common.InvalidNotificationid;
    private static ulong leaveLobbyRequestedNotificationId = Common.InvalidNotificationid;

    internal static void SetupOverlay()
    {
        var addNotifyLobbyInviteReceivedOptions = new AddNotifyLobbyInviteReceivedOptions();
        lobbyInviteReceivedNotificationId = EOSManager.LobbyInterface.AddNotifyLobbyInviteReceived(ref addNotifyLobbyInviteReceivedOptions, null, OnLobbyInviteReceived);
        var addNotifyLobbyInviteAcceptedOptions = new AddNotifyLobbyInviteAcceptedOptions();
        lobbyInviteAcceptedNotificationId = EOSManager.LobbyInterface.AddNotifyLobbyInviteAccepted(ref addNotifyLobbyInviteAcceptedOptions, null, OnLobbyInviteAccepted);

        var addNotifyJoinLobbyAcceptedOptions = new AddNotifyJoinLobbyAcceptedOptions();
        joinLobbyAcceptedNotificationId = EOSManager.LobbyInterface.AddNotifyJoinLobbyAccepted(ref addNotifyJoinLobbyAcceptedOptions, null, OnJoinLobbyAccepted);
        var addNotifyLeaveLobbyRequestedOptions = new AddNotifyLeaveLobbyRequestedOptions();
        leaveLobbyRequestedNotificationId = EOSManager.LobbyInterface.AddNotifyLeaveLobbyRequested(ref addNotifyLeaveLobbyRequestedOptions, null, OnLeaveLobbyRequested);
    }

    internal static void ShutdownOverlay()
    {
        if (lobbyInviteReceivedNotificationId != Common.InvalidNotificationid)
        {
            EOSManager.LobbyInterface.RemoveNotifyLobbyInviteReceived(lobbyInviteReceivedNotificationId);
            lobbyInviteReceivedNotificationId = Common.InvalidNotificationid;
        }
        if (lobbyInviteAcceptedNotificationId != Common.InvalidNotificationid)
        {
            EOSManager.LobbyInterface.RemoveNotifyLobbyInviteAccepted(lobbyInviteAcceptedNotificationId);
            lobbyInviteAcceptedNotificationId = Common.InvalidNotificationid;
        }

        if (joinLobbyAcceptedNotificationId != Common.InvalidNotificationid)
        {
            EOSManager.LobbyInterface.RemoveNotifyJoinLobbyAccepted(joinLobbyAcceptedNotificationId);
            joinLobbyAcceptedNotificationId = Common.InvalidNotificationid;
        }
        if (leaveLobbyRequestedNotificationId != Common.InvalidNotificationid)
        {
            EOSManager.LobbyInterface.RemoveNotifyLeaveLobbyRequested(leaveLobbyRequestedNotificationId);
            leaveLobbyRequestedNotificationId = Common.InvalidNotificationid;
        }
    }

    private static void OnLobbyInviteReceived(ref LobbyInviteReceivedCallbackInfo inviteInfo)
    {
#if DEBUG
        FusionLogger.Log($"Received lobby invite: {inviteInfo.InviteId}");
#endif
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
#if DEBUG
        FusionLogger.Log($"Accepted lobby invite: {inviteInfo.InviteId} for lobby {inviteInfo.LobbyId}");
#endif
        if (NetworkLayerManager.Layer is EOSNetworkLayer eosLayer)
        {
            var networkLayer = NetworkLayerManager.Layer as EOSNetworkLayer;
            networkLayer.JoinServer(inviteInfo.LobbyId);
        }
    }

    private static void OnJoinLobbyAccepted(ref JoinLobbyAcceptedCallbackInfo callbackInfo)
    {
        if (callbackInfo.UiEventId != UIInterface.EventidInvalid)
        {
            var copyDetailsOptions = new CopyLobbyDetailsHandleByUiEventIdOptions
            {
                UiEventId = callbackInfo.UiEventId
            };
            EOSManager.LobbyInterface.CopyLobbyDetailsHandleByUiEventId(ref copyDetailsOptions, out var lobbyDetails);

            var copyInfoOptions = new LobbyDetailsCopyInfoOptions();
            lobbyDetails.CopyInfo(ref copyInfoOptions, out var lobbyDetailsInfo);

            string lobbyId = lobbyDetailsInfo?.LobbyId;

            var acknowledgeOptions = new AcknowledgeEventIdOptions
            {
                UiEventId = callbackInfo.UiEventId,
                Result = Result.Success
            };
            EOSManager.UIInterface.AcknowledgeEventId(ref acknowledgeOptions);

            if (NetworkLayerManager.Layer is EOSNetworkLayer eosLayer)
            {
                eosLayer.JoinServer(lobbyId);
            }

#if DEBUG
            FusionLogger.Log($"Join lobby accepted from overlay: {lobbyId}");
#endif
        }
    }

    private static void OnLeaveLobbyRequested(ref LeaveLobbyRequestedCallbackInfo data)
    {
#if DEBUG
        FusionLogger.Log($"Leave lobby from overlay");
#endif

        if (NetworkLayerManager.Layer is EOSNetworkLayer eosLayer)
        {
            eosLayer.Disconnect();
        }
    }
}