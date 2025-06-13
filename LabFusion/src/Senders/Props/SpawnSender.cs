using LabFusion.Network;
using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Marrow.Patching;
using LabFusion.Marrow.Messages;

using Il2CppSLZ.Marrow.Warehouse;

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