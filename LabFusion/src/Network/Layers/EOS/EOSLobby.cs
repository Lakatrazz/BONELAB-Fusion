using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

using LabFusion.Utilities;

namespace LabFusion.Network
{
	public class EOSLobby : NetworkLobby
	{
		public LobbyDetails LobbyDetails { get; private set; }
		private ProductUserId _hostId;
		private string _lobbyId;

		public EOSLobby(LobbyDetails lobbyDetails, ProductUserId hostId, string lobbyId)
		{
			LobbyDetails = lobbyDetails;
			_hostId = hostId;
			_lobbyId = lobbyId;
		}

		public override void SetMetadata(string key, string value)
		{
			SaveKey(key);

			if (NetworkLayerManager.Layer is EOSNetworkLayer eosLayer && LobbyDetails != null)
			{
				var lobbyInterface = eosLayer._platformInterface.GetLobbyInterface();
				if (lobbyInterface == null)
				{
					FusionLogger.Error("Failed to get lobby interface for SetMetadata");
					return;
				}

				var updateOptions = new UpdateLobbyModificationOptions
				{
					LobbyId = _lobbyId,
					LocalUserId = EOSNetworkLayer.LocalUserId
				};

				Result result = lobbyInterface.UpdateLobbyModification(ref updateOptions, out LobbyModification lobbyModification);
				if (result != Result.Success || lobbyModification == null)
				{
					FusionLogger.Error($"Failed to create lobby modification: {result}");
					return;
				}

				try
				{
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
						FusionLogger.Error($"Failed to add attribute to lobby modification: {result}");
						return;
					}

					var updateLobbyOptions = new UpdateLobbyOptions
					{
						LobbyModificationHandle = lobbyModification
					};

					lobbyInterface.UpdateLobby(ref updateLobbyOptions, null, (ref UpdateLobbyCallbackInfo data) =>
					{
						if (data.ResultCode != Result.Success)
						{
							FusionLogger.Error($"Failed to update lobby with new attribute: {data.ResultCode}");
						}
					});
				}
				finally
				{
					lobbyModification.Release();
				}
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

			value = null;
			return false;
		}

		public override string GetMetadata(string key)
		{
			TryGetMetadata(key, out string value);
			return value;
		}

		public override Action CreateJoinDelegate(ulong lobbyId)
		{
			if (NetworkLayerManager.Layer is EOSNetworkLayer eosLayer)
			{
				var hostId = ProductUserId.FromString(lobbyId.ToString());

				if (hostId != null)
				{
					return () => eosLayer.JoinServer(hostId);
				}
			}

			return null;
		}

		public string GetLobbyId()
		{
			return _lobbyId;
		}
	}
}