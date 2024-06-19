using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Patching;
using LabFusion.Extensions;

using Il2CppSLZ.SFX;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Interaction;
using LabFusion.Entities;

namespace LabFusion.Network
{
    public class InventorySlotDropData : IFusionSerializable
    {
        public const int Size = sizeof(byte) * 5;

        public byte smallId;
        public byte grabber;
        public byte slotIndex;
        public Handedness handedness;

        public bool isAvatarSlot;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(grabber);
            writer.Write(slotIndex);
            writer.Write((byte)handedness);

            writer.Write(isAvatarSlot);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            grabber = reader.ReadByte();
            slotIndex = reader.ReadByte();
            handedness = (Handedness)reader.ReadByte();

            isAvatarSlot = reader.ReadBoolean();
        }

        public static InventorySlotDropData Create(byte smallId, byte grabber, byte slotIndex, Handedness handedness, bool isAvatarSlot = false)
        {
            return new InventorySlotDropData()
            {
                smallId = smallId,
                grabber = grabber,
                slotIndex = slotIndex,
                handedness = handedness,

                isAvatarSlot = isAvatarSlot,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class InventorySlotDropMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.InventorySlotDrop;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<InventorySlotDropData>();
            // Send message to other clients if server
            if (isServerHandled)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.grabber, NetworkChannel.Reliable, message, false);
            }
            else
            {
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
                    var slotReceiver = references.GetSlot(data.slotIndex, data.isAvatarSlot);
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
                        return;

                    if (NetworkPlayerManager.TryGetPlayer(data.grabber, out var grabber) && !grabber.NetworkEntity.IsOwner)
                    {
                        if (weaponSlot && weaponSlot.grip)
                        {
                            weaponSlot.grip.MoveIntoHand(grabber.RigReferences.GetHand(data.handedness));
                            grabber.Grabber.Attach(data.handedness, weaponSlot.grip);
                        }

                        var hand = grabber.RigReferences.GetHand(data.handedness);
                        if (hand)
                        {
                            hand.gameObject.GetComponent<HandSFX>().BodySlot();
                        }
                    }
                }
            }
        }
    }
}
