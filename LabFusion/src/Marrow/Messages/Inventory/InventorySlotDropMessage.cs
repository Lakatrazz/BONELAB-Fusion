using LabFusion.Marrow.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Utilities;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Marrow.Extenders;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Messages;

public class InventorySlotDropData : INetSerializable
{
    public const int Size = sizeof(byte) * 3 + sizeof(ushort);

    public int? GetSize() => Size;

    public ushort SlotEntityID;
    public byte GrabberID;
    public byte SlotIndex;
    public Handedness Handedness;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref SlotEntityID);
        serializer.SerializeValue(ref GrabberID);
        serializer.SerializeValue(ref SlotIndex);
        serializer.SerializeValue(ref Handedness, Precision.OneByte);
    }
}

[Net.SkipHandleWhileLoading]
public class InventorySlotDropMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<InventorySlotDropData>();

        var slotEntity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.SlotEntityID);

        if (slotEntity == null)
        {
            return;
        }

        var slotExtender = slotEntity.GetExtender<InventorySlotReceiverExtender>();

        if (slotExtender == null)
        {
            return;
        }

        var slotReceiver = slotExtender.GetComponent(data.SlotIndex);
        WeaponSlot weaponSlot = null;

        if (slotReceiver != null && slotReceiver._weaponHost != null)
        {
            weaponSlot = slotReceiver._slottedWeapon;

            slotReceiver._weaponHost.TryDetach();
        }

        InventorySlotReceiverPatches.IgnorePatches = true;

        try
        {
            slotReceiver.DropWeapon();
        }
        catch (Exception e)
        {
            FusionLogger.LogException("executing InventorySlotReceiver.DropWeapon", e);
        }

        InventorySlotReceiverPatches.IgnorePatches = false;

        if (data.Handedness == Handedness.UNDEFINED)
        {
            return;
        }

        if (NetworkPlayerManager.TryGetPlayer(data.GrabberID, out var grabber) && !grabber.NetworkEntity.IsOwner)
        {
            if (weaponSlot && weaponSlot.grip)
            {
                weaponSlot.grip.MoveIntoHand(grabber.RigRefs.GetHand(data.Handedness));
                grabber.Grabber.Attach(data.Handedness, weaponSlot.grip);
            }

            var hand = grabber.RigRefs.GetHand(data.Handedness);

            if (hand)
            {
                hand.gameObject.GetComponent<HandSFX>().BodySlot();
            }
        }
    }
}