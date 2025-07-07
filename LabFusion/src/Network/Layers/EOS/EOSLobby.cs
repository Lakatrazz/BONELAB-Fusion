using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

using LabFusion.Utilities;

namespace LabFusion.Network;

public class EOSLobby : NetworkLobby
{
	internal LobbyDetails LobbyDetails;
	internal readonly string LobbyId;

	private List<KeyValuePair<string, string>> _metadataCache = new();

	public EOSLobby(LobbyDetails lobbyDetails, string lobbyId)
	{
		LobbyDetails = lobbyDetails;
		LobbyId = lobbyId;
	}

	public override Action CreateJoinDelegate(string lobbyId)
	{
		if (NetworkLayerManager.Layer is EOSNetworkLayer eosLayer)
		{
			return () =>
			{
				eosLayer.JoinServer(lobbyId);
			};
		}

		return null;
	}

	public void UpdateLobbyDetails(LobbyDetails details)
	{
		LobbyDetails.Release();
		LobbyDetails = details;
	}

	public override void SetMetadata(string key, string value)
	{
		value ??= string.Empty;

		var keyValuePair = new KeyValuePair<string, string>(key, value);

		var existingPair = _metadataCache.FirstOrDefault(kvp => kvp.Key == key);
		if (!existingPair.Equals(default(KeyValuePair<string, string>)) && existingPair.Value == value)
		{
			// FusionLogger.Log($"Metadata key '{key}' already set with same value, skipping update.");
			return;
		}

		SaveKey(key);
		SetData(key, value);
	}

	public void SetData(string key, string value)
	{
		try
		{
			if (!NetworkInfo.IsHost)
				return;

			var keyValuePair = new KeyValuePair<string, string>(key, value);

			var updateOptions = new UpdateLobbyModificationOptions
			{
				LobbyId = LobbyId,
				LocalUserId = EOSNetworkLayer.LocalUserId
			};

			Result result = EOSManager.LobbyInterface.UpdateLobbyModification(ref updateOptions, out LobbyModification lobbyModification);
			if (result != Result.Success)
			{
				lobbyModification?.Release();
				throw new InvalidOperationException($"Failed to create lobby modification: {result}");
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

			result = lobbyModification.AddAttribute(ref addAttributeOptions);
			if (result != Result.Success)
			{
				throw new InvalidOperationException($"Failed to add attribute to lobby modification: {result}");
			}

			var updateLobbyOptions = new UpdateLobbyOptions
			{
				LobbyModificationHandle = lobbyModification
			};

			EOSManager.LobbyInterface.UpdateLobby(ref updateLobbyOptions, null, (ref UpdateLobbyCallbackInfo data) =>
			{
				if (data.ResultCode != Result.Success)
				{
					throw new InvalidOperationException($"Failed to update lobby with new attribute: {data.ResultCode}");
				}
			});

			_metadataCache.RemoveAll(kvp => kvp.Key == key);
			_metadataCache.Add(keyValuePair);

			lobbyModification.Release();
		}
		catch (Exception ex)
		{
			// A common exception is when the lobby gets closed but fusion tries to set metadata anyway.
			FusionLogger.Error($"Failed to set metadata for key '{key}' in lobby '{LobbyId}': {ex.Message}");
		}
	}

	public override bool TryGetMetadata(string key, out string value)
	{
		if (LobbyDetails != null)
		{
			var options = new LobbyDetailsCopyAttributeByKeyOptions
			{
				AttrKey = key
			};

			var result = LobbyDetails.CopyAttributeByKey(ref options, out Epic.OnlineServices.Lobby.Attribute? attribute);
			if (result == Result.Success && attribute.HasValue)
			{
				value = attribute.Value.Data?.Value.AsUtf8;
				return value != null;
			}
		}

		FusionLogger.Error($"Failed to get metadata for key '{key}' in lobby '{LobbyId}' since lobby details were null!");

		value = null;
		return false;
	}

	public override string GetMetadata(string key)
	{
		TryGetMetadata(key, out string value);
		return value;
	}
}