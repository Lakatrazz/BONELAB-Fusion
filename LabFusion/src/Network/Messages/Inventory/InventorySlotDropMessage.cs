using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

namespace LabFusion.Network;

public class InventorySlotDropData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 3 + sizeof(ushort);

    public ushort slotEntityId;
    public byte grabber;
    public byte slotIndex;
    public Handedness handedness;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(slotEntityId);
        writer.Write(grabber);
        writer.Write(slotIndex);
        writer.Write((byte)handedness);
    }

    public void Deserialize(FusionReader reader)
    {
        slotEntityId = reader.ReadUInt16();
        grabber = reader.ReadByte();
        slotIndex = reader.ReadByte();
        handedness = (Handedness)reader.ReadByte();
    }

    public static InventorySlotDropData Create(ushort slotEntityId, byte grabber, byte slotIndex, Handedness handedness)
    {
        return new InventorySlotDropData()
        {
            slotEntityId = slotEntityId,
            grabber = grabber,
            slotIndex = slotIndex,
            handedness = handedness,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class InventorySlotDropMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.InventorySlotDrop;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<InventorySlotDropData>();

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

        var slotReceiver = slotExtender.GetComponent(data.slotIndex);
        WeaponSlot weaponSlot = null;

        if (slotReceiver != null && slotReceiver._weaponHost != null)
        {
            weaponSlot = slotReceiver._slottedWeapon;

            slotReceiver._weaponHost.TryDetach();
        }

        InventorySlotReceiverPatches.IgnorePatches = true;

        slotReceiver.DropWeapon();

        InventorySlotReceiverPatches.IgnorePatches = false;

        if (data.handedness == Handedness.UNDEFINED)
        {
            return;
        }

        if (NetworkPlayerManager.TryGetPlayer(data.grabber, out var grabber) && !grabber.NetworkEntity.IsOwner)
        {
            if (weaponSlot && weaponSlot.grip)
            {
                weaponSlot.grip.MoveIntoHand(grabber.RigRefs.GetHand(data.handedness));
                grabber.Grabber.Attach(data.handedness, weaponSlot.grip);
            }

            var hand = grabber.RigRefs.GetHand(data.handedness);

            if (hand)
            {
                hand.gameObject.GetComponent<HandSFX>().BodySlot();
            }
        }
    }
}