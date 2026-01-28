using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

using LabFusion.Utilities;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Manages EOS lobby creation, joining, and lifecycle.
/// </summary>
internal class EOSLobbyManager
{
    private const int MaxLobbyMembers = 64;
    private const string BucketId = "FUSION";

    public static LobbyDetails CurrentLobbyDetails { get; private set; }

    private readonly ProductUserId _localUserId;

    public EpicLobby CurrentLobby { get; private set; }
    public bool IsInLobby => CurrentLobby != null;

    public EOSLobbyManager(ProductUserId localUserId)
    {
        _localUserId = localUserId ?? throw new ArgumentNullException(nameof(localUserId));
    }

    public void CreateLobby(Action<EpicLobby> onComplete)
    {
        var createOptions = new CreateLobbyOptions
        {
            BucketId = BucketId,
            DisableHostMigration = true,
            LocalUserId = _localUserId,
            MaxLobbyMembers = MaxLobbyMembers,
            PermissionLevel = LobbyPermissionLevel.Publicadvertised,
            EnableRTCRoom = false,
            PresenceEnabled = true,
            RejoinAfterKickRequiresInvite = false,
            EnableJoinById = true,
            AllowInvites = true,
        };

        EOSInterfaces.Lobby.CreateLobby(ref createOptions, null, (ref CreateLobbyCallbackInfo info) =>
        {
            if (info.ResultCode != Result.Success)
            {
                FusionLogger.Error($"Failed to create EOS lobby: {info.ResultCode}");
                onComplete?.Invoke(null);
                return;
            }
            
            var copyOptions = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = info.LobbyId,
                LocalUserId = _localUserId,
            };
            
            var result = EOSInterfaces.Lobby.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);
            if (result != Result.Success || lobbyDetails == null)
            {
                FusionLogger.Error($"Failed to copy lobby details handle: {result}");
                return;
            }

            var lobby = CreateLobbyFromInfo(lobbyDetails, info.LobbyId);
            onComplete?.Invoke(lobby);
        });
    }

    public void JoinLobby(LobbyDetails lobbyDetails, Action<EpicLobby> onComplete)
    {
        var joinOptions = new JoinLobbyOptions
        {
            CrossplayOptOut = false,
            LobbyDetailsHandle = lobbyDetails,
            LocalUserId = _localUserId,
            PresenceEnabled = false,
        };
        
        EOSInterfaces.Lobby.JoinLobby(ref joinOptions, null, (ref JoinLobbyCallbackInfo info) =>
        {
            if (info.ResultCode != Result.Success)
            {
                FusionLogger.Error($"Failed to join EOS lobby: {info.ResultCode}");
                onComplete?.Invoke(null);
                return;
            }

            var lobby = CreateLobbyFromInfo(lobbyDetails, info.LobbyId);
            onComplete?.Invoke(lobby);
        });
    }

    public void LeaveLobby(Action onComplete)
    {
        if (CurrentLobby == null)
        {
            onComplete?.Invoke();
            return;
        }

        var leaveOptions = new LeaveLobbyOptions
        {
            LocalUserId = _localUserId,
            LobbyId = CurrentLobby.LobbyId
        };

        EOSInterfaces.Lobby.LeaveLobby(ref leaveOptions, null, (ref LeaveLobbyCallbackInfo info) =>
        {
            CleanupCurrentLobby();
            onComplete?.Invoke();
        });
    }

    public void DestroyLobby(Action onComplete)
    {
        if (CurrentLobby == null)
        {
            onComplete?.Invoke();
            return;
        }

        var destroyOptions = new DestroyLobbyOptions
        {
            LocalUserId = _localUserId,
            LobbyId = CurrentLobby.LobbyId
        };

        EOSInterfaces.Lobby.DestroyLobby(ref destroyOptions, null, (ref DestroyLobbyCallbackInfo info) =>
        {
            CleanupCurrentLobby();
            onComplete?.Invoke();
        });
    }

    public void KickMember(string platformId, Action<bool> onComplete = null)
    {
        if (CurrentLobby == null)
        {
            onComplete?.Invoke(false);
            return;
        }

        var targetUserId = ProductUserId.FromString(platformId);
        if (targetUserId == null)
        {
            onComplete?.Invoke(false);
            return;
        }

        var kickOptions = new KickMemberOptions
        {
            LobbyId = CurrentLobby.LobbyId,
            LocalUserId = _localUserId,
            TargetUserId = targetUserId,
        };

        EOSInterfaces.Lobby.KickMember(ref kickOptions, null, (ref KickMemberCallbackInfo info) =>
        {
            if (info.ResultCode != Result.Success)
            {
#if DEBUG
                FusionLogger.Error($"Failed to kick member: {info.ResultCode}");
#endif
            }
            onComplete?.Invoke(info.ResultCode == Result.Success);
        });
    }

    public ProductUserId GetLobbyOwner()
    {
        if (CurrentLobbyDetails == null)
            return null;

        var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();
        return CurrentLobbyDetails.GetLobbyOwner(ref ownerOptions);
    }

    private EpicLobby CreateLobbyFromInfo(LobbyDetails lobbyDetails, string lobbyId)
    {
        CurrentLobbyDetails = lobbyDetails;
        CurrentLobby = new EpicLobby(lobbyDetails, lobbyId);

#if DEBUG
        FusionLogger.Log($"Lobby ready: {lobbyId}");
#endif

        return CurrentLobby;
    }

    private void CleanupCurrentLobby()
    {
        CurrentLobby?.Release();
        CurrentLobby = null;
        CurrentLobbyDetails = null;
    }
}