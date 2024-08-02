using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;

namespace LabFusion.Network;

public class InventorySlotInsertData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 4 + sizeof(ushort);

    public byte smallId;
    public byte inserter;
    public ushort syncId;
    public byte slotIndex;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(inserter);
        writer.Write(syncId);
        writer.Write(slotIndex);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        inserter = reader.ReadByte();
        syncId = reader.ReadUInt16();
        slotIndex = reader.ReadByte();
    }

    public static InventorySlotInsertData Create(byte smallId, byte inserter, ushort syncId, byte slotIndex)
    {
        return new InventorySlotInsertData()
        {
            smallId = smallId,
            inserter = inserter,
            syncId = syncId,
            slotIndex = slotIndex,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class InventorySlotInsertMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.InventorySlotInsert;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<InventorySlotInsertData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag, bytes);
            MessageSender.BroadcastMessageExcept(data.inserter, NetworkChannel.Reliable, message, false);

            return;
        }

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

        if (!NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
        {
            return;
        }

        var slotExtender = player.NetworkEntity.GetExtender<InventorySlotReceiverExtender>();

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