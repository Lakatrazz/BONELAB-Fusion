using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

using LabFusion.Utilities;

using MelonLoader;

using System;
using System.Collections;
using System.Collections.Generic;

namespace LabFusion.Network
{
	public sealed class EOSMatchmaker : IMatchmaker
	{
		private LobbyInterface _lobbyInterface;

		public EOSMatchmaker(LobbyInterface lobbyInterface)
		{
			_lobbyInterface = lobbyInterface;
		}

		public void RequestLobbies(Action<IMatchmaker.MatchmakerCallbackInfo> callback)
		{
			MelonCoroutines.Start(FindLobbies(callback));
		}

		private IEnumerator FindLobbies(Action<IMatchmaker.MatchmakerCallbackInfo> callback)
		{
			if (EOSNetworkLayer.LocalUserId == null || _lobbyInterface == null)
			{
				FusionLogger.Error("Cannot find lobbies: LocalUserId or LobbyInterface is null");
				callback?.Invoke(new IMatchmaker.MatchmakerCallbackInfo { Lobbies = new IMatchmaker.LobbyInfo[0] });
				yield break;
			}

			var createSearchOptions = new CreateLobbySearchOptions
			{
				MaxResults = 100,
			};

			Result createResult = _lobbyInterface.CreateLobbySearch(ref createSearchOptions, out LobbySearch searchHandle);
			if (createResult != Result.Success || searchHandle == null)
			{
				FusionLogger.Error($"Failed to create lobby search: {createResult}");
				callback?.Invoke(new IMatchmaker.MatchmakerCallbackInfo { Lobbies = new IMatchmaker.LobbyInfo[0] });
				yield break;
			}

			var paramOptions = new LobbySearchSetParameterOptions
			{
				Parameter = new AttributeData
				{
					Key = "lobby_open",
                    Value = new AttributeDataValue { AsUtf8 = bool.TrueString }
				},
				ComparisonOp = ComparisonOp.Equal,
			};

			searchHandle.SetParameter(ref paramOptions);

			var findOptions = new LobbySearchFindOptions
			{
				LocalUserId = EOSNetworkLayer.LocalUserId
			};

			bool searchComplete = false;
			Result searchResult = Result.Success;

			searchHandle.Find(ref findOptions, null, (ref LobbySearchFindCallbackInfo info) =>
			{
				searchResult = info.ResultCode;
				searchComplete = true;
			});

			while (!searchComplete)
			{
				yield return null;
			}

			if (searchResult != Result.Success)
			{
				FusionLogger.Error($"Failed to find lobbies: {searchResult}");
				searchHandle.Release();
				callback?.Invoke(new IMatchmaker.MatchmakerCallbackInfo { Lobbies = new IMatchmaker.LobbyInfo[0] });
				yield break;
			}

			var lobbies = new List<IMatchmaker.LobbyInfo>();

			var countOptions = new LobbySearchGetSearchResultCountOptions();
			uint lobbyCount = searchHandle.GetSearchResultCount(ref countOptions);

            for (uint i = 0; i < lobbyCount; i++)
			{
				var copyOptions = new LobbySearchCopySearchResultByIndexOptions
				{
					LobbyIndex = i
				};

				if (searchHandle.CopySearchResultByIndex(ref copyOptions, out LobbyDetails lobbyDetails) == Result.Success && lobbyDetails != null)
				{
					var infoOptions = new LobbyDetailsCopyInfoOptions();
					Result infoResult = lobbyDetails.CopyInfo(ref infoOptions, out LobbyDetailsInfo? lobbyInfo);

					if (infoResult == Result.Success && lobbyInfo.HasValue)
					{
						var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();
						ProductUserId ownerId = lobbyDetails.GetLobbyOwner(ref ownerOptions);

						if (ownerId != null)
						{
							var networkLobby = new EOSLobby(lobbyDetails, lobbyInfo.Value.LobbyId);

							var metadata = LobbyMetadataHelper.ReadInfo(networkLobby);

							if (metadata.HasServerOpen)
							{
								lobbies.Add(new IMatchmaker.LobbyInfo
								{
									Lobby = networkLobby,
									Metadata = metadata
								});
							}
						} else
						{
							FusionLogger.Error($"Failed to get lobby owner for lobby index {i} since owner ID is null!");
                        }
					}
				} else
				{
					FusionLogger.Error($"Failed to copy search result for lobby index {i}");
                }
			}

			searchHandle.Release();

			FusionLogger.Log($"Found {lobbies.Count} lobbies");

			callback?.Invoke(new IMatchmaker.MatchmakerCallbackInfo
			{
				Lobbies = lobbies.ToArray()
			});
		}
	}
}