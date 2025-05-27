using LabFusion.Marrow.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

namespace LabFusion.Network;

public class InventorySlotDropData : INetSerializable
{
    public const int Size = sizeof(byte) * 3 + sizeof(ushort);

    public ushort slotEntityId;
    public byte grabber;
    public byte slotIndex;
    public Handedness handedness;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref slotEntityId);
        serializer.SerializeValue(ref grabber);
        serializer.SerializeValue(ref slotIndex);
        serializer.SerializeValue(ref handedness, Precision.OneByte);
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

[Net.SkipHandleWhileLoading]
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

        try
        {
            slotReceiver.DropWeapon();
        }
        catch (Exception e)
        {
            FusionLogger.LogException("executing InventorySlotReceiver.DropWeapon", e);
        }

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