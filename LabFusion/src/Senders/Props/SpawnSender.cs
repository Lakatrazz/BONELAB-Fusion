using LabFusion.Network;
using LabFusion.Data;

namespace LabFusion.Senders;

public static class SpawnSender
{
    /// <summary>
    /// Sends a catchup sync message for a pool spawned object.
    /// </summary>
    /// <param name="syncable"></param>
    public static void SendCatchupSpawn(byte owner, string barcode, ushort syncId, SerializedTransform serializedTransform, byte playerID)
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        var data = SpawnResponseData.Create(owner, barcode, syncId, serializedTransform);

        MessageRelay.RelayNative(data, NativeMessageTag.SpawnResponse, new MessageRoute(playerID, NetworkChannel.Reliable));
    }
}