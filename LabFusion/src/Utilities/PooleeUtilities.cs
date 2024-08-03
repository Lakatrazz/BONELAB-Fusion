using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;

using LabFusion.Entities;

namespace LabFusion.Utilities;

public static class PooleeUtilities
{
    internal static bool CanDespawn = false;

    public static void DespawnAll()
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        // Loop through all NetworkProps and despawn them
        var entities = NetworkEntityManager.IdManager.RegisteredEntities.EntityIdLookup.Keys.ToArray();
        foreach (var networkEntity in entities)
        {
            var prop = networkEntity.GetExtender<NetworkProp>();

            if (prop == null)
            {
                continue;
            }

            var poolee = networkEntity.GetExtender<PooleeExtender>();

            if (poolee == null)
            {
                continue;
            }

            poolee.Component.Despawn();
        }
    }

    public static void SendDespawn(ushort syncId)
    {
        // Send response
        if (NetworkInfo.IsServer)
        {
            using var writer = FusionWriter.Create(DespawnResponseData.Size);
            var data = DespawnResponseData.Create(syncId, PlayerIdManager.LocalSmallId);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.DespawnResponse, writer);
            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
        }
        // Send request
        else
        {
            using var writer = FusionWriter.Create(DespawnRequestData.Size);
            var data = DespawnRequestData.Create(syncId, PlayerIdManager.LocalSmallId);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.DespawnRequest, writer);
            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
        }
    }

    public static void RequestDespawn(ushort syncId, bool isMag = false)
    {
        using var writer = FusionWriter.Create(DespawnRequestData.Size);
        var data = DespawnRequestData.Create(syncId, PlayerIdManager.LocalSmallId, isMag);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.DespawnRequest, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }

    public static void RequestSpawn(string barcode, SerializedTransform serializedTransform, uint trackerId)
    {
        using var writer = FusionWriter.Create(SpawnRequestData.Size);
        var data = SpawnRequestData.Create(PlayerIdManager.LocalSmallId, barcode, serializedTransform, trackerId);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.SpawnRequest, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }

    public static void SendSpawn(byte owner, string barcode, ushort syncId, SerializedTransform serializedTransform, uint trackerId = 0)
    {
        using var writer = FusionWriter.Create(SpawnResponseData.GetSize(barcode));
        var data = SpawnResponseData.Create(owner, barcode, syncId, serializedTransform, trackerId);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.SpawnResponse, writer);

        MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
    }
}