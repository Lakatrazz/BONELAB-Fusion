using LabFusion.Entities;
using LabFusion.Marrow;

using Il2CppSLZ.Marrow.Audio;

using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class InventoryAmmoReceiverDropData : INetSerializable
{
    public const int Size = sizeof(ushort);

    public ushort entityId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref entityId);
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

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<InventoryAmmoReceiverDropData>();

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