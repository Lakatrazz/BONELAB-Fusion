using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;

using SLZ.Interaction;

using UnityEngine;
using LabFusion.Extensions;
using SLZ;

namespace LabFusion.Data
{
    public class StaticGrabGroupHandler : GrabGroupHandler<SerializedStaticGrab>
    {
        public override GrabGroup? Group => GrabGroup.STATIC;
    }

    public class SerializedStaticGrab : SerializedGrab
    {
        public string fullPath;
        public SerializedTransform worldHand = null;

        public SerializedStaticGrab() { }

        public SerializedStaticGrab(string fullPath) {
            this.fullPath = fullPath;
        }

        public override void WriteDefaultGrip(Hand hand, Grip grip) {
            base.WriteDefaultGrip(hand, grip);

            worldHand = new SerializedTransform(hand.transform);
        }

        public override void Serialize(FusionWriter writer) {
            base.Serialize(writer);

            writer.Write(fullPath);
            writer.Write(worldHand);
        }

        public override void Deserialize(FusionReader reader) {
            base.Deserialize(reader);

            fullPath = reader.ReadString();
            worldHand = reader.ReadFusionSerializable<SerializedTransform>();
        }

        public override Grip GetGrip() {
            var go = GameObjectUtilities.GetGameObject(fullPath);

            if (go) {
                var grip = Grip.Cache.Get(go);
                return grip;
            }

            return null;
        }

        public override void RequestGrab(PlayerRep rep, Handedness handedness, Grip grip)
        {
            // Don't do anything if this isn't grabbed anymore
            if (!isGrabbed)
                return;

            // Get the hand and its starting values
            Hand hand = rep.RigReferences.GetHand(handedness);

            Transform handTransform = hand.transform;
            Vector3 position = handTransform.position;
            Quaternion rotation = handTransform.rotation;

            // Move the hand into its world position
            handTransform.SetPositionAndRotation(worldHand.position, worldHand.rotation.Expand());

            // Apply the grab
            base.RequestGrab(rep, handedness, grip);

            // Reset the hand position
            handTransform.SetPositionAndRotation(position, rotation);
        }
    }
}
