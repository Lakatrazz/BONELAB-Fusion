using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using LabFusion.Utilities;

namespace LabFusion.Network;

internal class EOSLobby : EOSInterface
{
    internal EOSRuntime Runtime;
    internal LobbyInterface LobbyInterface;
    internal ProductUserId LocalUserId;
    internal EpicLobby CurrentLobby;

    private bool joinInProgress = false;
    
    internal EOSLobby(EOSRuntime eosRuntime, LobbyInterface lobbyInterface, ProductUserId localUserId)
    {
        Runtime = eosRuntime;
        LobbyInterface = lobbyInterface;
        LocalUserId = localUserId;
    }

    internal void CreateLobby()
    {
        var createLobbyOptions = new CreateLobbyOptions
        {
            BucketId = "Fusion",
            DisableHostMigration = true,
            LocalUserId = LocalUserId,
            MaxLobbyMembers = 64,
            PermissionLevel = LobbyPermissionLevel.Publicadvertised,
            EnableRTCRoom = false,
            PresenceEnabled = false,
            RejoinAfterKickRequiresInvite = false,
            EnableJoinById = true,
            AllowInvites = true,
        };
        
        LobbyInterface.CreateLobby(ref createLobbyOptions, null, (ref CreateLobbyCallbackInfo info) =>
        {
            if (info.ResultCode != Result.Success)
            {
                FusionLogger.Error($"Failed to create EOS lobby: {info.ResultCode}");
                return;
            }
            
            var copyOptions = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = info.LobbyId,
                LocalUserId = LocalUserId,
            };
            
            var result = LobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);
            if (result != Result.Success || lobbyDetails == null)
            {
                FusionLogger.Error($"Failed to copy lobby details handle: {result}");
                return;
            }
            
            CurrentLobby = new EpicLobby(Runtime, lobbyDetails, info.LobbyId, LocalUserId);
            
            // Manually call a metadata write
            LobbyMetadataSerializer.WriteInfo(CurrentLobby);
        });
    }

    internal void JoinLobby(EpicLobby epicLobby)
    {
        // Idiot proof the join button
        if (joinInProgress)
        {
            FusionLogger.Warn("Join lobby already in progress");
            return;
        }
        
        var joinLobbyOptions = new JoinLobbyOptions
        {
            CrossplayOptOut = false,
            LobbyDetailsHandle = epicLobby.LobbyDetails,
            LocalUserId = LocalUserId,
            PresenceEnabled = false,
        };
        
        LobbyInterface.JoinLobby(ref joinLobbyOptions, null, (ref JoinLobbyCallbackInfo info) =>
        {
            if (info.ResultCode != Result.Success)
            {
                FusionLogger.Error($"Failed to join EOS lobby: {info.ResultCode}");
                NetworkHelper.Disconnect();
                CurrentLobby = null;
            }

            joinInProgress = false;
        });
        
        CurrentLobby = epicLobby;
        joinInProgress = true;
    }

    internal void LeaveLobby()
    {
        if (joinInProgress)
        {
            FusionLogger.Warn("Cannot leave lobby while join is in progress");
            return;
        }

        if (CurrentLobby == null)
        {
            FusionLogger.Warn("No current lobby to leave");
            return;
        }

        if (CurrentLobby.Owner == LocalUserId)
        {
            Destroy();
        }
        else
        {
            Leave();
        }
        
        CurrentLobby = null;

        void Destroy()
        {
            var destroyLobbyOptions = new DestroyLobbyOptions
            {
                LocalUserId = LocalUserId,
                LobbyId = CurrentLobby.LobbyID
            };
            
            LobbyInterface.DestroyLobby(ref destroyLobbyOptions, null, (ref DestroyLobbyCallbackInfo info) =>
            {
                if (info.ResultCode != Result.Success)
                {
                    FusionLogger.Error($"Failed to destroy lobby: {info.ResultCode}");
                }
            });
        }
        
        void Leave()
        {
            var leaveLobbyOptions = new LeaveLobbyOptions
            {
                LocalUserId = LocalUserId,
                LobbyId = CurrentLobby.LobbyID
            };
            
            LobbyInterface.LeaveLobby(ref leaveLobbyOptions, null, (ref LeaveLobbyCallbackInfo info) =>
            {
                if (info.ResultCode != Result.Success && info.ResultCode != Result.NotFound)
                {
                    FusionLogger.Error($"Failed to leave lobby: {info.ResultCode}");
                }
            });
        }
    }

    // Why this doesnt just fucking use LobbyDetails is beyond me
    internal bool SetAttribute(Utf8String lobbyId, string key, string value)
    {
        var updateLobbyModificationOptions = new UpdateLobbyModificationOptions
        {
            LobbyId = lobbyId,
            LocalUserId = LocalUserId,
        };
        
        var updateLobbyModificationResult = LobbyInterface.UpdateLobbyModification(ref updateLobbyModificationOptions, out var modification);
        if (updateLobbyModificationResult != Result.Success || modification == null)
        {
            FusionLogger.Error($"Failed to create lobby modification: {updateLobbyModificationResult}");
            modification?.Release();
            return false;
        }
        
        var attributeData = new AttributeData
        {
            Key = key,
            Value = new AttributeDataValue { AsUtf8 = value }
        };
        var lobbyModificationAddAttributeOptions = new LobbyModificationAddAttributeOptions
        {
            Attribute = attributeData,
            Visibility = LobbyAttributeVisibility.Public
        };

        var addAttributeResult = modification.AddAttribute(ref lobbyModificationAddAttributeOptions);
        if (addAttributeResult != Result.Success)
        {
            FusionLogger.Error($"Failed to add attribute '{key}': {addAttributeResult}");
            modification.Release();
            return false;
        }
        
        var updateLobbyOptions = new UpdateLobbyOptions
        {
            LobbyModificationHandle = modification
        };
        
        LobbyInterface.UpdateLobby(ref updateLobbyOptions, null, (ref UpdateLobbyCallbackInfo info) =>
        {
            if (info.ResultCode != Result.Success)
            {
                FusionLogger.Error($"Failed to update lobby attribute '{key}': {info.ResultCode}");
            }
            else
            {
#if DEBUG
                FusionLogger.Log($"Successfully updated lobby attribute '{key}'");
#endif
            }
            modification.Release();
        });
        
        return true;
    }
    
    internal string GetAttribute(LobbyDetails lobbyDetails, string key)
    {
        var lobbyDetailsCopyAttributeByKeyOptions = new LobbyDetailsCopyAttributeByKeyOptions
        {
            AttrKey = key
        };
        
        var result = lobbyDetails.CopyAttributeByKey(ref lobbyDetailsCopyAttributeByKeyOptions, out var attribute);
        if (result == Result.Success && attribute.HasValue)
        {
            return attribute.Value.Data?.Value.AsUtf8 ?? string.Empty;
        }
        
        return string.Empty;
    }
}