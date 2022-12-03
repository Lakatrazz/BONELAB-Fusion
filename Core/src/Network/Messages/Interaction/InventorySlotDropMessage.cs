using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;

using SLZ;
using SLZ.Interaction;
using LabFusion.Syncables;
using SLZ.SFX;
using LabFusion.Patching;
using SLZ.Props.Weapons;

namespace LabFusion.Network
{
    public class InventorySlotDropData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public byte grabber;
        public byte slotIndex;
        public Handedness handedness;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(grabber);
            writer.Write(slotIndex);
            writer.Write((byte)handedness);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            grabber = reader.ReadByte();
            slotIndex = reader.ReadByte();
            handedness = (Handedness)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static InventorySlotDropData Create(byte smallId, byte grabber, byte slotIndex, Handedness handedness)
        {
            return new InventorySlotDropData()
            {
                smallId = smallId,
                grabber = grabber,
                slotIndex = slotIndex,
                handedness = handedness,
            };
        }
    }

    [Net.DelayWhileLoading]
    public class InventorySlotDropMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.InventorySlotDrop;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<InventorySlotDropData>()) {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.grabber, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        FusionLogger.Log("Receive message thing!");
                        RigReferenceCollection references = null;

                        if (data.smallId == PlayerIdManager.LocalSmallId) {
                            references = RigData.RigReferences;
                        }
                        else if (PlayerRep.Representations.TryGetValue(data.smallId, out var rep)) {
                            references = rep.RigReferences;
                        }

                        if (references != null) {
                            var slotReceiver = references.GetSlot(data.slotIndex);
                            WeaponSlot weaponSlot = null;

                            if (slotReceiver != null && slotReceiver._weaponHost != null) {
                                weaponSlot = slotReceiver._slottedWeapon;
                                slotReceiver._weaponHost.ForceDetach();
                            }

                            InventorySlotReceiverGrab.PreventDropCheck = true;
                            slotReceiver.DropWeapon();
                            InventorySlotReceiverGrab.PreventDropCheck = false;

                            if (PlayerRep.Representations.TryGetValue(data.grabber, out var grabber)) {
                                if (weaponSlot && weaponSlot.grip)
                                    grabber.AttachObject(data.handedness, weaponSlot.grip);

                                var hand = grabber.RigReferences.GetHand(data.handedness);
                                if (hand) {
                                    HandSFX.Cache.Get(hand.gameObject).BodySlot();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
