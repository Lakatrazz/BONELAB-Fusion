using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Extensions;
using LabFusion.Grabbables;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ;
using SLZ.Interaction;

using UnityEngine;

namespace LabFusion.Data {
    public class PlayerGrabGroupHandler : GrabGroupHandler<SerializedPlayerBodyGrab> {
        public override GrabGroup? Group => GrabGroup.PLAYER_BODY;
    }

    public class SerializedPlayerBodyGrab : SerializedGrab {
        public byte grabbedUser;
        public byte gripIndex;

        public SerializedTransform relativeGrip = null;

        public SerializedPlayerBodyGrab(byte grabbedUser, byte gripIndex, GripPair pair) {
            this.grabbedUser = grabbedUser;
            this.gripIndex = gripIndex;

            var handTransform = pair.hand.transform;
            var gripTransform = pair.grip.Host.GetTransform();

            relativeGrip = new SerializedTransform(handTransform.InverseTransformPoint(gripTransform.position), handTransform.InverseTransformRotation(gripTransform.rotation));
        }

        public SerializedPlayerBodyGrab() { }

        public override void Serialize(FusionWriter writer) {
            base.Serialize(writer);

            writer.Write(grabbedUser);
            writer.Write(gripIndex);
            writer.Write(relativeGrip);
        }

        public override void Deserialize(FusionReader reader) {
            base.Deserialize(reader);

            grabbedUser = reader.ReadByte();
            gripIndex = reader.ReadByte();
            relativeGrip = reader.ReadFusionSerializable<SerializedTransform>();
        }

        public override Grip GetGrip() {
            RigReferenceCollection references = null;
            if (grabbedUser == PlayerIdManager.LocalSmallId)
                references = RigData.RigReferences;
            else if (PlayerRepManager.TryGetPlayerRep(grabbedUser, out var rep))
                references = rep.RigReferences;

            return references?.GetGrip(gripIndex);
        }

        public override void RequestGrab(PlayerRep rep, Handedness handedness, Grip grip, bool useCustomJoint = true)
        {
            // Don't do anything if this isn't grabbed anymore
            if (!isGrabbed)
                return;

            // Set the host position so that the grip is created in the right spot
            var host = grip.Host.GetTransform();
            Vector3 position = host.position;
            Quaternion rotation = host.rotation;

            if (relativeGrip != null) {
                var hand = rep.RigReferences.GetHand(handedness).transform;

                host.SetPositionAndRotation(hand.TransformPoint(relativeGrip.position), hand.TransformRotation(relativeGrip.rotation.Expand()));
            }

            // There is no need for custom joints on player grips
            // Since friction works properly and the grab is attached in the same spot
            useCustomJoint = false;

            // Apply the grab
            base.RequestGrab(rep, handedness, grip, useCustomJoint);

            // Reset the host position
            host.position = position;
            host.rotation = rotation;
        }
    }
}
