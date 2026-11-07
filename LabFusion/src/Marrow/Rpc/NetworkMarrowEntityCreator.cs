using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Entities;
using LabFusion.Marrow.Messages;
using LabFusion.Network;

namespace LabFusion.Marrow.Rpc;

/// <summary>
/// Functions for requesting marrow entities to be synchronized on the server.
/// </summary>
public static class NetworkMarrowEntityCreator
{
    /// <summary>
    /// Callback information after a scene marrow entity was successfully networked.
    /// </summary>
    public struct MarrowEntityCreateCallbackInfo
    {
        /// <summary>
        /// The marrow entity that was synchronized.
        /// </summary>
        public MarrowEntity MarrowEntity;

        /// <summary>
        /// The network entity that now exists on the server.
        /// </summary>
        public NetworkEntity NetworkEntity;
    }

    /// <summary>
    /// Information for requesting a scene marrow entity to be networked.
    /// </summary>
    public struct MarrowEntityCreateRequestInfo
    {
        /// <summary>
        /// The marrow entity to network.
        /// </summary>
        public MarrowEntity MarrowEntity;

        /// <summary>
        /// An optional callback that will be invoked locally when the network entity has been created.
        /// </summary>
        public Action<MarrowEntityCreateCallbackInfo> CreatedCallback;
    }

    private static uint _lastTrackerID = 0;

    private static readonly Dictionary<uint, Action<MarrowEntityCreateCallbackInfo>> _callbackByID = new();

    /// <summary>
    /// Sends a request to the server to create a NetworkEntity for a MarrowEntity in the scene. This can only be called by clients and is not guaranteed to be successful.
    /// </summary>
    /// <param name="info"></param>
    public static void SendCreateRequest(MarrowEntityCreateRequestInfo info)
    {
        if (!ClientManager.IsClientConnected)
        {
            return;
        }

        var hashData = MarrowEntityHelper.GetDataFromEntity(info.MarrowEntity);

        if (hashData == null)
        {
            return;
        }

        var trackerID = _lastTrackerID++;

        var requestData = new MarrowEntityCreateRequestData()
        {
            HashData = hashData,
            TrackerID = trackerID,
        };

        if (info.CreatedCallback != null)
        {
            _callbackByID[trackerID] = info.CreatedCallback;
        }

        ClientManager.SendToServerModule<MarrowEntityCreateRequest, MarrowEntityCreateRequestData>(requestData, NetworkChannel.Reliable);
    }

    internal static void OnCreated(uint trackerID, MarrowEntityCreateCallbackInfo callbackInfo)
    {
        if (!_callbackByID.TryGetValue(trackerID, out var callback))
        {
            return;
        }

        _callbackByID.Remove(trackerID);

        callback(callbackInfo);
    }
}
