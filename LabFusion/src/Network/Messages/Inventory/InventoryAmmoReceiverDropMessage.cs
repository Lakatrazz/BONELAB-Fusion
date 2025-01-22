using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow;

using Il2CppSLZ.Marrow.Audio;

namespace LabFusion.Network;

public class InventoryAmmoReceiverDropData : IFusionSerializable
{
    public const int Size = sizeof(ushort);

    public ushort entityId;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(entityId);
    }

    public void Deserialize(FusionReader reader)
    {
        entityId = reader.ReadUInt16();
    }

    public static InventoryAmmoReceiverDropData Create(ushort entityId)
    {
        return new InventoryAmmoReceiverDropData()
        {
            entityId = entityId,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class InventoryAmmoReceiverDropMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.InventoryAmmoReceiverDrop;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<InventoryAmmoReceiverDropData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag, bytes);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
            return;
        }

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.entityId);

        if (entity == null)
        {
            return;
        }

        var ammoReceiverExtender = entity.GetExtender<InventoryAmmoReceiverExtender>();

        if (ammoReceiverExtender == null)
        {
            return;
        }

        var ammoReceiver = ammoReceiverExtender.Component;

        SafeAudio3dPlayer.PlayAtPoint(ammoReceiver.grabClips, ammoReceiver.transform.position, Audio3dManager.softInteraction, 0.2f);
    }
}