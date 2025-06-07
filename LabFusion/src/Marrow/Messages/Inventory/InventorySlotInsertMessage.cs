using LabFusion.Marrow.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Utilities;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Marrow.Extenders;

namespace LabFusion.Marrow.Messages;

public class InventorySlotInsertData : INetSerializable
{
    public const int Size = sizeof(ushort) * 2 + sizeof(byte);

    public ushort SlotEntityID;
    public ushort WeaponID;
    public byte SlotIndex;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref SlotEntityID);
        serializer.SerializeValue(ref WeaponID);
        serializer.SerializeValue(ref SlotIndex);
    }
}

[Net.SkipHandleWhileLoading]
public class InventorySlotInsertMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<InventorySlotInsertData>();

        var weaponEntity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.WeaponID);

        if (weaponEntity == null)
        {
            return;
        }

        var slotEntity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.SlotEntityID);

        if (slotEntity == null)
        {
            return;
        }

        var slotEntityExtender = slotEntity.GetExtender<IMarrowEntityExtender>();

        if (slotEntityExtender == null)
        {
            return;
        }

        slotEntityExtender.HookOnReady(OnSlotReady);

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
                slotExtender.GetComponent(data.SlotIndex).InsertInSlot(weaponExtender.Component.interactableHost);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("handling InventorySlotInsertMessage", e);
            }

            InventorySlotReceiverPatches.IgnorePatches = false;
        }
    }
}