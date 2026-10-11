using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using LabFusion.Network;

namespace MarrowFusion.Epic;

internal class EOSLobby : EOSInterface
{
    internal EOSRuntime Runtime;
    internal LobbyInterface LobbyInterface;
    internal ProductUserId LocalUserId;
    
    internal EpicLobby CurrentLobby;
    
    internal EOSLobby(EOSRuntime eosRuntime, LobbyInterface lobbyInterface, ProductUserId localUserId)
    {
        Runtime = eosRuntime;
        LobbyInterface = lobbyInterface;
        LocalUserId = localUserId;
    }
    
    internal void CreateLobby()
    {
        var createOptions = new CreateLobbyOptions
        {
            BucketId = "Marrow Fusion",
            DisableHostMigration = true,
            LocalUserId = LocalUserId,
            MaxLobbyMembers = 1,
            PermissionLevel = LobbyPermissionLevel.Publicadvertised,
            EnableRTCRoom = false,
            PresenceEnabled = false,
            RejoinAfterKickRequiresInvite = false,
            EnableJoinById = true,
            AllowInvites = true,
        };
        
        LobbyInterface.CreateLobby(ref createOptions, null, (ref CreateLobbyCallbackInfo info) =>
        {
            // Frequently happens when the game freezes during level reload
            if (info.ResultCode == Result.TimedOut)
            {
                EpicModule.Logger.Warn("Lobby creation timed out, retrying...");
                CreateLobby();
                return;
            }
            
            if (info.ResultCode != Result.Success)
            {
                EpicModule.Logger.Error($"Failed to create EOS lobby: {info.ResultCode}");
                NetworkManager.DisconnectClientAndServer();
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
                EpicModule.Logger.Error($"Failed to copy lobby details handle: {result}");
                NetworkManager.DisconnectClientAndServer();
                return;
            }
            
            CurrentLobby = new EpicLobby(Runtime, lobbyDetails, LocalUserId);
        });
    }

    private bool destroyLobbyPending;
    internal void DestroyLobby()
    {
        if (destroyLobbyPending)
            return;
        
        if (CurrentLobby == null)
        {
            EpicModule.Logger.Warn("No current lobby to destroy");
            return;
        }
        
        if (CurrentLobby.Owner != LocalUserId)
        {
            EpicModule.Logger.Warn("Cannot destroy lobby, not the owner");
            return;
        }
        
        var copyInfoOptions = new LobbyDetailsCopyInfoOptions();
        
        var copyInfoResult = CurrentLobby.LobbyDetails.CopyInfo(ref copyInfoOptions, out var lobbyInfo);
        if (copyInfoResult != Result.Success || lobbyInfo == null)
        {
            EpicModule.Logger.Error($"Failed to copy lobby info: {copyInfoResult}");
            return;
        }
        
        destroyLobbyPending = true;

        var destroyOptions = new DestroyLobbyOptions
        {
            LocalUserId = LocalUserId,
            LobbyId = lobbyInfo.Value.LobbyId,
        };
            
        LobbyInterface.DestroyLobby(ref destroyOptions, null, (ref DestroyLobbyCallbackInfo info) =>
        {
            if (info.ResultCode == Result.AlreadyPending)
            {
                EpicModule.Logger.Warn("Lobby destruction already pending, waiting for completion...");
                return;
            }
            
            if (info.ResultCode != Result.Success && info.ResultCode != Result.NotFound)
            {
                EpicModule.Logger.Error($"Failed to destroy lobby: {info.ResultCode}");
                destroyLobbyPending = false;
                return;
            }
            
            CurrentLobby = null;
            destroyLobbyPending = false;
        });
    }
    
    internal bool SetAttribute(LobbyDetails lobbyDetails, string key, string value)
    {
        var copyInfoOptions = new LobbyDetailsCopyInfoOptions();
        
        var copyInfoResult = lobbyDetails.CopyInfo(ref copyInfoOptions, out var lobbyInfo);
        if (copyInfoResult != Result.Success || lobbyInfo == null)
        {
            EpicModule.Logger.Error($"Failed to copy lobby info: {copyInfoResult}");
            return false;
        }
        
        var lobbyModificationOptions = new UpdateLobbyModificationOptions
        {
            LobbyId = lobbyInfo.Value.LobbyId,
            LocalUserId = LocalUserId,
        };
        
        var updateLobbyModificationResult = LobbyInterface.UpdateLobbyModification(ref lobbyModificationOptions, out var modification);
        if (updateLobbyModificationResult != Result.Success || modification == null)
        {
            EpicModule.Logger.Error($"Failed to create lobby modification: {updateLobbyModificationResult}");
            modification?.Release();
            return false;
        }
        
        var attributeData = new AttributeData
        {
            Key = key,
            Value = new AttributeDataValue { AsUtf8 = value }
        };
        var addAttributeOptions = new LobbyModificationAddAttributeOptions
        {
            Attribute = attributeData,
            Visibility = LobbyAttributeVisibility.Public
        };

        var addAttributeResult = modification.AddAttribute(ref addAttributeOptions);
        if (addAttributeResult != Result.Success)
        {
            EpicModule.Logger.Error($"Failed to add attribute '{key}': {addAttributeResult}");
            modification.Release();
            return false;
        }
        
        var updateLobbyOptions = new UpdateLobbyOptions
        {
            LobbyModificationHandle = modification
        };
        
        LobbyInterface.UpdateLobby(ref updateLobbyOptions, null, (ref UpdateLobbyCallbackInfo info) =>
        {
            if (info.ResultCode != Result.Success && info.ResultCode != Result.NotFound && info.ResultCode != Result.UnexpectedError)
            {
                EpicModule.Logger.Error($"Failed to update lobby attribute '{key}': {info.ResultCode}");
            }
            
            modification.Release();
        });
        
        return true;
    }
    
    internal string GetAttribute(LobbyDetails lobbyDetails, string key)
    {
        var copyAttributeOptions = new LobbyDetailsCopyAttributeByKeyOptions
        {
            AttrKey = key
        };
        
        var result = lobbyDetails.CopyAttributeByKey(ref copyAttributeOptions, out var attribute);
        if (result == Result.Success && attribute.HasValue)
        {
            return attribute.Value.Data?.Value.AsUtf8 ?? string.Empty;
        }
        
        return string.Empty;
    }
}