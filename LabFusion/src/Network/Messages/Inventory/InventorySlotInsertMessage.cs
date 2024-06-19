using LabFusion.Data;
using LabFusion.Representation;
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

    public bool isAvatarSlot;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(inserter);
        writer.Write(syncId);
        writer.Write(slotIndex);

        writer.Write(isAvatarSlot);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        inserter = reader.ReadByte();
        syncId = reader.ReadUInt16();
        slotIndex = reader.ReadByte();

        isAvatarSlot = reader.ReadBoolean();
    }

    public static InventorySlotInsertData Create(byte smallId, byte inserter, ushort syncId, byte slotIndex, bool isAvatarSlot = false)
    {
        return new InventorySlotInsertData()
        {
            smallId = smallId,
            inserter = inserter,
            syncId = syncId,
            slotIndex = slotIndex,

            isAvatarSlot = isAvatarSlot,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class InventorySlotInsertMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.InventorySlotInsert;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<InventorySlotInsertData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.inserter, NetworkChannel.Reliable, message, false);

            return;
        }

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.syncId);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<WeaponSlotExtender>();

        if (extender == null)
        {
            return;
        }

        RigReferenceCollection references = null;

        if (data.smallId == PlayerIdManager.LocalSmallId)
        {
            references = RigData.RigReferences;
        }
        else if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var rep))
        {
            references = rep.RigReferences;
        }

        if (references != null)
        {
            extender.Component.interactableHost.TryDetach();

            InventorySlotReceiverDrop.PreventInsertCheck = true;
            references.GetSlot(data.slotIndex, data.isAvatarSlot).InsertInSlot(extender.Component.interactableHost);
            InventorySlotReceiverDrop.PreventInsertCheck = false;
        }
    }
}