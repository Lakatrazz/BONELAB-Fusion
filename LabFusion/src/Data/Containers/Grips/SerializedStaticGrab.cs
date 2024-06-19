using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Extensions;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Interaction;

using UnityEngine;
using LabFusion.Entities;

namespace LabFusion.Data
{
    public class StaticGrabGroupHandler : GrabGroupHandler<SerializedStaticGrab>
    {
        public override GrabGroup? Group => GrabGroup.STATIC;
    }

    public class SerializedStaticGrab : SerializedGrab
    {
        public new const int Size = SerializedGrab.Size + SerializedTransform.Size;

        public string fullPath;
        public SerializedTransform worldHand = default;

        public SerializedStaticGrab() { }

        public SerializedStaticGrab(string fullPath)
        {
            this.fullPath = fullPath;
        }

        public override int GetSize()
        {
            return Size + fullPath.GetSize();
        }

        public override void WriteDefaultGrip(Hand hand, Grip grip)
        {
            base.WriteDefaultGrip(hand, grip);

            worldHand = new SerializedTransform(hand.transform);
        }

        public override void Serialize(FusionWriter writer)
        {
            base.Serialize(writer);

            writer.Write(fullPath);
            writer.Write(worldHand);
        }

        public override void Deserialize(FusionReader reader)
        {
            base.Deserialize(reader);

            fullPath = reader.ReadString();
            worldHand = reader.ReadFusionSerializable<SerializedTransform>();
        }

        public override Grip GetGrip()
        {
            var go = GameObjectUtilities.GetGameObject(fullPath);

            if (go)
            {
                var grip = Grip.Cache.Get(go);
                return grip;
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
