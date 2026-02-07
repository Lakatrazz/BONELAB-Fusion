using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Represents an EOS lobby with metadata management.
/// </summary>
public class EpicLobby : NetworkLobby
{
    private LobbyDetails _lobbyDetails;
    private readonly Dictionary<string, string> _metadataCache = new();
    private readonly Dictionary<string, string> _pendingMetadata = new();
    private readonly object _lock = new();

    private bool _isUpdating;
    private bool _hasPendingChanges;

    public string LobbyId { get; }
    public LobbyDetails LobbyDetails => _lobbyDetails;

    public EpicLobby(LobbyDetails lobbyDetails, string lobbyId)
    {
        _lobbyDetails = lobbyDetails ?? throw new ArgumentNullException(nameof(lobbyDetails));
        LobbyId = lobbyId ?? throw new ArgumentNullException(nameof(lobbyId));
    }

    internal void Release()
    {
        _lobbyDetails?.Release();
        _lobbyDetails = null;
    }

    public override void SetMetadata(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
            return;

        value ??= string.Empty;

        if (!NetworkInfo.IsHost)
            return;

        lock (_lock)
        {
            // Check if value actually changed
            if (_metadataCache.TryGetValue(key, out var cached) && cached == value)
                return;

            // Queue the change
            _pendingMetadata[key] = value;
            _hasPendingChanges = true;

            SaveKey(key);

            // If not currently updating, start the update
            if (!_isUpdating)
            {
                FlushPendingMetadata();
            }
        }
    }

    /// <summary>
    /// Flushes all pending metadata changes in a single batch update.
    /// </summary>
    private void FlushPendingMetadata()
    {
        Dictionary<string, string> changesToApply;

        lock (_lock)
        {
            if (!_hasPendingChanges || _pendingMetadata.Count == 0)
                return;

            _isUpdating = true;
            _hasPendingChanges = false;

            // Copy pending changes and clear
            changesToApply = new Dictionary<string, string>(_pendingMetadata);
            _pendingMetadata.Clear();
        }

        ApplyMetadataBatch(changesToApply);
    }

    private void ApplyMetadataBatch(Dictionary<string, string> changes)
    {
        var lobbyInterface = EOSInterfaces.Lobby;
        if (lobbyInterface == null)
        {
            FusionLogger.Error("LobbyInterface is null, cannot set metadata");
            CompleteUpdate(success: false, changes);
            return;
        }

        var localUserId = ProductUserId.FromString(PlayerIDManager.LocalPlatformID);
        if (localUserId == null)
        {
            FusionLogger.Error("Local user ID is null, cannot set metadata");
            CompleteUpdate(success: false, changes);
            return;
        }

        // Create a single lobby modification
        var updateOptions = new UpdateLobbyModificationOptions
        {
            LobbyId = LobbyId,
            LocalUserId = localUserId
        };

        var result = lobbyInterface.UpdateLobbyModification(ref updateOptions, out var modification);
        if (result != Result.Success || modification == null)
        {
            FusionLogger.Error($"Failed to create lobby modification: {result}");
            CompleteUpdate(success: false, changes);
            return;
        }

        try
        {
            // Add all attributes to the single modification
            foreach (var kvp in changes)
            {
                var addResult = AddAttributeToModification(modification, kvp.Key, kvp.Value);
                if (addResult != Result.Success)
                {
                    FusionLogger.Error($"Failed to add attribute '{kvp.Key}':  {addResult}");
                    // Continue with other attributes
                }
            }

            // Apply the modification
            var applyOptions = new UpdateLobbyOptions
            {
                LobbyModificationHandle = modification
            };

            lobbyInterface.UpdateLobby(ref applyOptions, changes, OnLobbyUpdateComplete);
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("applying lobby metadata batch", ex);
            modification?.Release();
            CompleteUpdate(success: false, changes);
        }
    }

    private static Result AddAttributeToModification(LobbyModification modification, string key, string value)
    {
        var attributeData = new AttributeData
        {
            Key = key,
            Value = new AttributeDataValue { AsUtf8 = value }
        };

        var addOptions = new LobbyModificationAddAttributeOptions
        {
            Attribute = attributeData,
            Visibility = LobbyAttributeVisibility.Public
        };

        return modification.AddAttribute(ref addOptions);
    }

    private void OnLobbyUpdateComplete(ref UpdateLobbyCallbackInfo info)
    {
        var changes = info.ClientData as Dictionary<string, string>;

        if (info.ResultCode == Result.Success)
        {
            // Update cache with successful changes
            if (changes != null)
            {
                lock (_lock)
                {
                    foreach (var kvp in changes)
                    {
                        _metadataCache[kvp.Key] = kvp.Value;
                    }
                }
            }

#if DEBUG
            FusionLogger.Log($"Lobby metadata updated successfully ({changes?.Count ?? 0} attributes)");
#endif
        }
        else
        {
            FusionLogger.Error($"Failed to update lobby metadata: {info.ResultCode}");
        }

        CompleteUpdate(info.ResultCode == Result.Success, changes);
    }

    private void CompleteUpdate(bool success, Dictionary<string, string> appliedChanges)
    {
        lock (_lock)
        {
            _isUpdating = false;

            // If there are more pending changes, flush them
            if (_hasPendingChanges)
            {
                FlushPendingMetadata();
            }
        }
    }

    public override bool TryGetMetadata(string key, out string value)
    {
        value = string.Empty;

        if (_lobbyDetails == null)
            return false;

        var options = new LobbyDetailsCopyAttributeByKeyOptions
        {
            AttrKey = key
        };

        var result = _lobbyDetails.CopyAttributeByKey(ref options, out var attribute);

        if (result == Result.Success && attribute.HasValue)
        {
            value = attribute.Value.Data?.Value.AsUtf8 ?? string.Empty;
            return !string.IsNullOrEmpty(value);
        }

        return false;
    }

    public override string GetMetadata(string key)
    {
        TryGetMetadata(key, out var value);
        return value;
    }

    public override Action CreateJoinDelegate(string lobbyId)
    {
        if (NetworkLayerManager.Layer is EpicGamesNetworkLayer eosLayer)
        {
            return () => eosLayer.JoinServer(_lobbyDetails);
        }
        return null;
    }
}