using LabFusion.Extensions;
using LabFusion.Network;

using SLZ;
using SLZ.Interaction;
using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data
{
    public class SerializedGripAnchor : IFusionSerializable
    {
        public Handedness handedness;
        public Vector3 anchor;
        public Vector3 connectedAnchor;
        public SerializedQuaternion relativeRotation;

        public SerializedGripAnchor() { }

        public SerializedGripAnchor(Hand hand, Grip grip) {
            handedness = hand.handedness;

            if (hand.joint) {
                Vector3 origin = hand.transform.position;

                anchor = hand.joint.GetLocalAnchor(origin);
                connectedAnchor = hand.joint.GetLocalConnectedAnchor(origin);
                relativeRotation = SerializedQuaternion.Compress(Quaternion.Inverse(grip.transform.rotation) * hand.transform.rotation);
            }
        }

        public void CopyTo(Hand hand, Grip grip, ConfigurableJoint clientJoint) {
            if (hand.joint && clientJoint) {
                grip.FreeJoints(hand);

                // Update client joint
                clientJoint.autoConfigureConnectedAnchor = false;
                clientJoint.connectedBody = hand.joint.connectedBody;

                // Pos anchors
                clientJoint.anchor = anchor;
                clientJoint.connectedAnchor = connectedAnchor;

                // Rot anchors
                var initialRot = hand.transform.rotation;
                hand.transform.rotation = grip.transform.rotation * relativeRotation.Expand();

                clientJoint.swapBodies = !clientJoint.swapBodies;
                clientJoint.swapBodies = !clientJoint.swapBodies;

                hand.transform.rotation = initialRot;

                // Motion and drives
                clientJoint.linearLimit = new SoftJointLimit() { limit = 0.5f };
                clientJoint.xDrive = clientJoint.yDrive = clientJoint.zDrive = new JointDrive() { positionSpring = 5000000f, positionDamper = 100000f, maximumForce = 5000000f };

                clientJoint.xMotion = clientJoint.yMotion = clientJoint.zMotion 
                    = clientJoint.angularXMotion = clientJoint.angularYMotion = clientJoint.angularZMotion = ConfigurableJointMotion.Limited;
            }
        }

        public void FreeJoint(ConfigurableJoint joint) {
            joint.xDrive = joint.yDrive = joint.zDrive = joint.angularXDrive = joint.angularYZDrive = new JointDrive();
            joint.xMotion = joint.yMotion = joint.zMotion =
                joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Free;
        }

        public void Serialize(FusionWriter writer) {
            writer.Write((byte)handedness);
            writer.Write(anchor);
            writer.Write(connectedAnchor);
            writer.Write(relativeRotation);
        }

        public void Deserialize(FusionReader reader) {
            handedness = (Handedness)reader.ReadByte();
            anchor = reader.ReadVector3();
            connectedAnchor = reader.ReadVector3();
            relativeRotation = reader.ReadFusionSerializable<SerializedQuaternion>();
        }
    }
}
