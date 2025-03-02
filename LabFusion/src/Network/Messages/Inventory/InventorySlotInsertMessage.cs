using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class InventorySlotInsertData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort) * 2;

    public ushort slotEntityId;
    public byte inserter;
    public ushort syncId;
    public byte slotIndex;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref slotEntityId);
        serializer.SerializeValue(ref inserter);
        serializer.SerializeValue(ref syncId);
        serializer.SerializeValue(ref slotIndex);
    }

    public static InventorySlotInsertData Create(ushort slotEntityId, byte inserter, ushort syncId, byte slotIndex)
    {
        return new InventorySlotInsertData()
        {
            slotEntityId = slotEntityId,
            inserter = inserter,
            syncId = syncId,
            slotIndex = slotIndex,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class InventorySlotInsertMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.InventorySlotInsert;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<InventorySlotInsertData>();

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.syncId);

        if (entity == null)
        {
            return;
        }

        var weaponExtender = entity.GetExtender<WeaponSlotExtender>();

        if (weaponExtender == null)
        {
            return;
        }

        var slotEntity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.slotEntityId);

        if (slotEntity == null)
        {
            return;
        }

        var slotExtender = slotEntity.GetExtender<InventorySlotReceiverExtender>();

        if (slotExtender == null)
        {
            return;
        }

        weaponExtender.Component.interactableHost.TryDetach();

        InventorySlotReceiverDrop.PreventInsertCheck = true;

        slotExtender.GetComponent(data.slotIndex).InsertInSlot(weaponExtender.Component.interactableHost);

        InventorySlotReceiverDrop.PreventInsertCheck = false;
    }
}