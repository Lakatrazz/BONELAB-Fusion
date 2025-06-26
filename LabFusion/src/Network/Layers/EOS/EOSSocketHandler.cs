using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

using LabFusion.Utilities;

using System;
using System.Collections.Generic;

namespace LabFusion.Network
{
	public static class EOSSocketHandler
	{
		public const int MaxPacketSize = 1170;

		public static readonly SocketId SocketId = new SocketId { SocketName = "FusionSocket" };

		private static int _totalSentPackets = 0;
		private static int _totalReceivedPackets = 0;
		private static int _failedSendPackets = 0;

		public static byte ConvertToP2PChannel(NetworkChannel channel)
		{
			return (byte)channel;
		}

		public static void InitializeP2PConnection(P2PInterface p2pInterface, ProductUserId localUserId, ProductUserId remoteUserId)
		{
			if (p2pInterface == null || localUserId == null || remoteUserId == null)
			{
				FusionLogger.Error("Cannot initialize P2P connection - missing required objects");
				return;
			}

			if (localUserId.ToString() == remoteUserId.ToString())
			{
				return;
			}

			try
			{
				FusionLogger.Log($"Initializing P2P connection to {remoteUserId}");

				var connectOptions = new AddNotifyPeerConnectionRequestOptions
				{
					LocalUserId = localUserId,
					SocketId = SocketId
				};

				p2pInterface.AddNotifyPeerConnectionRequest(ref connectOptions, null, OnPeerConnectionRequest);

				var sendOptions = new SendPacketOptions
				{
					LocalUserId = localUserId,
					RemoteUserId = remoteUserId,
					SocketId = SocketId,
					Channel = 0,
					Data = new ArraySegment<byte>(new byte[] { 0 }),
					AllowDelayedDelivery = true,
					Reliability = PacketReliability.ReliableOrdered
				};

				p2pInterface.SendPacket(ref sendOptions);

				FusionLogger.Log($"P2P connection request sent to {remoteUserId}");
			}
			catch (Exception ex)
			{
				FusionLogger.LogException($"Error initializing P2P connection", ex);
			}
		}

		private static void OnPeerConnectionRequest(ref OnIncomingConnectionRequestInfo info)
		{
			FusionLogger.Log($"Received P2P connection request from {info.RemoteUserId}");

			if (NetworkLayerManager.Layer is EOSNetworkLayer eosLayer)
			{
				eosLayer.HandleP2PConnectionRequest(info.RemoteUserId);
			}
		}

		public static void CloseP2PConnection(P2PInterface p2pInterface, ProductUserId localUserId, ProductUserId remoteUserId)
		{
			if (p2pInterface == null || localUserId == null || remoteUserId == null)
			{
				return;
			}

			try
			{
				FusionLogger.Log($"Closing P2P connection to {remoteUserId}");

				var closeOptions = new CloseConnectionOptions
				{
					LocalUserId = localUserId,
					RemoteUserId = remoteUserId,
					SocketId = SocketId
				};

				Result result = p2pInterface.CloseConnection(ref closeOptions);

				if (result != Result.Success)
				{
					FusionLogger.Error($"Failed to close P2P connection: {result}");
				}
				else
				{
					FusionLogger.Log($"Successfully closed P2P connection to {remoteUserId}");
				}
			}
			catch (Exception ex)
			{
				FusionLogger.LogException($"Error closing P2P connection", ex);
			}
		}

		public static void SendMessageToConnection(P2PInterface p2pInterface, ProductUserId remoteUserId, NetworkChannel channel, NetMessage message)
		{
			if (p2pInterface == null || EOSNetworkLayer.LocalUserId == null || remoteUserId == null)
			{
				FusionLogger.Error("Cannot send message - missing required objects");
				_failedSendPackets++;
				return;
			}

			try
			{
				byte p2pChannel = ConvertToP2PChannel(channel);
				bool reliable = channel == NetworkChannel.Reliable;

				byte[] messageBytes = message.ToByteArray();

				if (messageBytes.Length == 0)
				{
					FusionLogger.Error("Message is empty, not sending");
					_failedSendPackets++;
					return;
				}

				if (messageBytes.Length > MaxPacketSize)
				{
					FusionLogger.Warn($"Message size ({messageBytes.Length}) exceeds max packet size ({MaxPacketSize}), may be truncated");
				}

				ArraySegment<byte> dataSegment = new ArraySegment<byte>(messageBytes);

				var sendOptions = new SendPacketOptions
				{
					LocalUserId = EOSNetworkLayer.LocalUserId,
					RemoteUserId = remoteUserId,
					SocketId = SocketId,
					Channel = p2pChannel,
					Data = dataSegment,
					AllowDelayedDelivery = true,
					Reliability = reliable ? PacketReliability.ReliableOrdered : PacketReliability.UnreliableUnordered
				};

				Result result = p2pInterface.SendPacket(ref sendOptions);

				if (result != Result.Success)
				{
					FusionLogger.Error($"Failed to send packet: {result}");
					_failedSendPackets++;
				}
				else
				{
					_totalSentPackets++;
					FusionLogger.Log($"Successfully sent packet #{_totalSentPackets} to {remoteUserId}, size: {messageBytes.Length}");
				}
			}
			catch (Exception ex)
			{
				_failedSendPackets++;
				FusionLogger.LogException("Error sending packet", ex);
			}
		}

		public static void BroadcastToClients(P2PInterface p2pInterface, NetworkChannel channel, NetMessage message, List<ProductUserId> connectedClients)
		{
			if (connectedClients == null || connectedClients.Count == 0)
			{
				FusionLogger.Log("No clients to broadcast to");
				return;
			}

			FusionLogger.Log($"Broadcasting to {connectedClients.Count} clients");

			bool reliable = channel == NetworkChannel.Reliable;
			int successCount = 0;

			foreach (var clientUserId in connectedClients)
			{
				if (clientUserId == null)
				{
					FusionLogger.Warn("Skipping null client in broadcast");
					continue;
				}

				FusionLogger.Log($"Sending message to client {clientUserId} on channel {channel} (reliable: {reliable})");

				if (EOSNetworkLayer.LocalUserId != null && clientUserId.ToString() == EOSNetworkLayer.LocalUserId.ToString())
				{
					FusionLogger.Warn($"Skipping self in broadcast: {EOSNetworkLayer.LocalUserId}");
					continue;
				}

				SendMessageToConnection(p2pInterface, clientUserId, channel, message);
				successCount++;
			}

			if (NetworkLayerManager.Layer != null && NetworkLayerManager.Layer.IsHost)
			{
				byte[] messageBytes = message.ToByteArray();
				var messageSpan = new ReadOnlySpan<byte>(messageBytes);

				var readableMessage = new ReadableMessage()
				{
					Buffer = messageSpan,
					IsServerHandled = true
				};

				NativeMessageHandler.ReadMessage(readableMessage);
			}

			FusionLogger.Log($"Broadcast completed to {successCount}/{connectedClients.Count} clients");
		}

		public static void ReceiveMessages(P2PInterface p2pInterface, int maxMessages)
		{
			if (p2pInterface == null || EOSNetworkLayer.LocalUserId == null)
			{
				return;
			}

			try
			{
				bool isHost = NetworkLayerManager.Layer?.IsHost ?? false;

				for (int i = 0; i < maxMessages; i++)
				{
					var getNextReceivedPacketSizeOptions = new GetNextReceivedPacketSizeOptions
					{
						LocalUserId = EOSNetworkLayer.LocalUserId,
						RequestedChannel = null
					};

					if (p2pInterface.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint nextPacketSize) != Result.Success)
					{
						break;
					}

					if (nextPacketSize == 0)
					{
						continue;
					}

					byte[] buffer = new byte[nextPacketSize];
					ArraySegment<byte> dataSegment = new ArraySegment<byte>(buffer);

					var receiveOptions = new ReceivePacketOptions
					{
						LocalUserId = EOSNetworkLayer.LocalUserId,
						MaxDataSizeBytes = nextPacketSize,
						RequestedChannel = null
					};

					ProductUserId peerId = null;
					SocketId socketId = new SocketId();
					byte channel = 0;

					Result result = p2pInterface.ReceivePacket(ref receiveOptions, ref peerId, ref socketId, out channel, dataSegment, out uint bytesWritten);

					if (result == Result.Success && bytesWritten > 0)
					{
						_totalReceivedPackets++;

						if (peerId != null)
						{
							NetworkInfo.LastReceivedUser = (ulong)peerId.ToString().GetHashCode();
						}

						var messageSpan = new ReadOnlySpan<byte>(buffer, 0, (int)bytesWritten);

						var readableMessage = new ReadableMessage()
						{
							Buffer = messageSpan,
							IsServerHandled = isHost
						};

						NativeMessageHandler.ReadMessage(readableMessage);
					}
					else if (result != Result.Success)
					{
						FusionLogger.Error($"Failed to receive packet: {result}");
						break;
					}
				}
			}
			catch (Exception ex)
			{
				FusionLogger.LogException("Error receiving P2P messages", ex);
			}
		}
	}
}