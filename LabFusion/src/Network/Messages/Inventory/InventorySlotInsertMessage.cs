using LabFusion.Marrow.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class InventorySlotInsertData : INetSerializable
{
    public const int Size = sizeof(ushort) * 2 + sizeof(byte);

    public ushort slotEntityId;
    public ushort weaponId;
    public byte slotIndex;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref slotEntityId);
        serializer.SerializeValue(ref weaponId);
        serializer.SerializeValue(ref slotIndex);
    }

    public static InventorySlotInsertData Create(ushort slotEntityId, ushort weaponId, byte slotIndex)
    {
        return new InventorySlotInsertData()
        {
            slotEntityId = slotEntityId,
            weaponId = weaponId,
            slotIndex = slotIndex,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class InventorySlotInsertMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.InventorySlotInsert;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<InventorySlotInsertData>();

        NetworkEntity weaponEntity = null;
        NetworkEntity slotEntity = null;

        NetworkEntityManager.HookEntityRegistered(data.weaponId, OnWeaponRegistered);

        void OnWeaponRegistered(NetworkEntity entity)
        {
            weaponEntity = entity;

            NetworkEntityManager.HookEntityRegistered(data.slotEntityId, OnSlotRegistered);
        }

        void OnSlotRegistered(NetworkEntity entity)
        {
            slotEntity = entity;

            var slotEntityExtender = slotEntity.GetExtender<IMarrowEntityExtender>();

            if (slotEntityExtender == null)
            {
                return;
            }

            slotEntityExtender.HookOnReady(OnSlotReady);
        }

        void OnSlotReady()
        {
            var weaponExtender = weaponEntity.GetExtender<WeaponSlotExtender>();

            if (weaponExtender == null)
            {
                return;
            }

            var slotExtender = slotEntity.GetExtender<InventorySlotReceiverExtender>();

            if (slotExtender == null)
            {
                return;
            }

            weaponExtender.Component.interactableHost.TryDetach();

            InventorySlotReceiverPatches.IgnorePatches = true;

            try
            {
                slotExtender.GetComponent(data.slotIndex).InsertInSlot(weaponExtender.Component.interactableHost);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("handling InventorySlotInsertMessage", e);
            }

            InventorySlotReceiverPatches.IgnorePatches = false;
        }
    }
}