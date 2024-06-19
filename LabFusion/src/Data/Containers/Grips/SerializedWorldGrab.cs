using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Grabbables;

using Il2CppSLZ.Interaction;

using UnityEngine;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;

namespace LabFusion.Data
{
    public class WorldGrabGroupHandler : GrabGroupHandler<SerializedWorldGrab>
    {
        public override GrabGroup? Group => GrabGroup.WORLD_GRIP;
    }

    public class SerializedWorldGrab : SerializedGrab
    {
        public new const int Size = SerializedGrab.Size + sizeof(byte) + SerializedTransform.Size;

        public byte grabberId;
        public SerializedTransform worldHand = default;

        public SerializedWorldGrab() { }

        public SerializedWorldGrab(byte grabberId)
        {
            this.grabberId = grabberId;
        }

        public override int GetSize()
        {
            return Size;
        }

        public override void WriteDefaultGrip(Hand hand, Grip grip)
        {
            base.WriteDefaultGrip(hand, grip);

            worldHand = new SerializedTransform(hand.transform);
        }

        public override void Serialize(FusionWriter writer)
        {
            base.Serialize(writer);

            writer.Write(grabberId);
            writer.Write(worldHand);
        }

        public override void Deserialize(FusionReader reader)
        {
            base.Deserialize(reader);

            grabberId = reader.ReadByte();
            worldHand = reader.ReadFusionSerializable<SerializedTransform>();
        }

        public override Grip GetGrip()
        {
            if (NetworkPlayerManager.TryGetPlayer(grabberId, out var player))
            {
                if (player.RigReferences.RigManager)
                {
                    var worldGrip = player.RigReferences.RigManager.worldGrip;
                    return worldGrip;
                }
            }

            return null;
        }

        public override void RequestGrab(NetworkPlayer player, Handedness handedness, Grip grip)
        {
            // Don't do anything if this isn't grabbed anymore
            if (!isGrabbed)
                return;

            // Get the hand and its starting values
            Hand hand = player.RigReferences.GetHand(handedness);

            Transform handTransform = hand.transform;
            Vector3 position = handTransform.position;
            Quaternion rotation = handTransform.rotation;

            // Move the hand into its world position
            handTransform.SetPositionAndRotation(worldHand.position, worldHand.rotation);

            // Apply the grab
            base.RequestGrab(player, handedness, grip);

            // Reset the hand position
            handTransform.SetPositionAndRotation(position, rotation);
        }
    }
}
