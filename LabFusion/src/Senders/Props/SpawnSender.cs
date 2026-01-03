using LabFusion.Network;
using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Serialization;

namespace LabFusion.Senders;

public static class SpawnSender
{
    /// <summary>
    /// Sends a catchup sync message for a pool spawned object.
    /// </summary>
    /// <param name="syncable"></param>
    public static void SendCatchupSpawn(byte ownerID, string barcode, ushort entityID, SerializedTransform serializedTransform, byte playerID, EntitySource source)
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        var data = new SpawnResponseData()
        {
            OwnerID = ownerID,
            EntityID = entityID,
            SpawnData = new SerializedSpawnData()
            {
                Barcode = barcode,
                SerializedTransform = serializedTransform,
                SpawnEffect = false,
                SpawnSource = source,
            }
        };

        MessageRelay.RelayNative(data, NativeMessageTag.SpawnResponse, new MessageRoute(playerID, NetworkChannel.Reliable));
    }
}