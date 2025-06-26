using Epic.OnlineServices;

namespace LabFusion.Network
{
	/// <summary>
	/// Represents an EOS P2P connection to a remote peer
	/// </summary>
	public class EOSConnection
	{
		/// <summary>
		/// The EOS ProductUserId of the remote peer
		/// </summary>
		public ProductUserId RemoteUserId { get; private set; }

		/// <summary>
		/// Whether this connection is to the local player
		/// </summary>
		public bool IsLocal { get; private set; }

		/// <summary>
		/// Whether this connection is to the host
		/// </summary>
		public bool IsHost { get; private set; }

		/// <summary>
		/// Platform ID for this connection (used for player identification)
		/// </summary>
		public ulong PlatformId { get; private set; }

		/// <summary>
		/// Whether the connection is active
		/// </summary>
		public bool IsActive { get; private set; } = true;

		public EOSConnection(ProductUserId remoteUserId, bool isLocal, bool isHost)
		{
			RemoteUserId = remoteUserId;
			IsLocal = isLocal;
			IsHost = isHost;

			PlatformId = (ulong)remoteUserId.ToString().GetHashCode();
		}

		public void Close()
		{
			IsActive = false;
		}
	}
}